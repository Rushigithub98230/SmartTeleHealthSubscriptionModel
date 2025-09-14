using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ChatSessionRepository : RepositoryBase<ChatSession>, IChatSessionRepository
{
    private readonly ApplicationDbContext _context;

    public ChatSessionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<ChatSession?> GetByIdAsync(object id)
    {
        if (id is not Guid sessionId)
            return null;

        return await _context.ChatSessions
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Subscription)
            .Include(c => c.Messages)
            .Include(c => c.Attachments)
            .FirstOrDefaultAsync(c => c.Id == sessionId && !c.IsDeleted);
    }

    public override async Task<IEnumerable<ChatSession>> GetAllAsync()
    {
        return await _context.ChatSessions
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Subscription)
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatSession>> GetByUserIdAsync(int userId)
    {
        return await _context.ChatSessions
            .Include(c => c.Provider)
            .Include(c => c.Subscription)
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatSession>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.ChatSessions
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Where(c => c.SubscriptionId == subscriptionId && !c.IsDeleted)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatSession>> GetByProviderIdAsync(int providerId)
    {
        return await _context.ChatSessions
            .Include(c => c.User)
            .Include(c => c.Subscription)
            .Where(c => c.ProviderId == providerId && !c.IsDeleted)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }

    public override async Task<ChatSession> CreateAsync(ChatSession session)
    {
        session.CreatedDate = DateTime.UtcNow;
        session.UpdatedDate = DateTime.UtcNow;
        return await base.CreateAsync(session);
    }

    public override async Task<ChatSession> UpdateAsync(ChatSession session)
    {
        session.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(session);
    }

    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid sessionId)
            return false;

        var session = await _context.ChatSessions.FindAsync(sessionId);
        if (session == null) return false;

        session.IsDeleted = true;
        session.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid sessionId)
            return false;

        return await _context.ChatSessions.AnyAsync(c => c.Id == sessionId && !c.IsDeleted);
    }

    public async Task<int> GetMonthlySessionCountAsync(Guid subscriptionId, DateTime startDate, DateTime endDate)
    {
        return await _context.ChatSessions
            .Where(c => c.SubscriptionId == subscriptionId && 
                       c.StartTime >= startDate && 
                       c.StartTime <= endDate && 
                       !c.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetMonthlyTotalMinutesAsync(Guid subscriptionId, DateTime startDate, DateTime endDate)
    {
        return await _context.ChatSessions
            .Where(c => c.SubscriptionId == subscriptionId && 
                       c.StartTime >= startDate && 
                       c.StartTime <= endDate && 
                       !c.IsDeleted)
            .SumAsync(c => c.DurationMinutes);
    }

    public async Task<int> GetActiveSessionCountAsync(int userId)
    {
        return await _context.ChatSessions
            .Where(c => c.UserId == userId && 
                       c.Status == ChatSession.ChatStatus.Active && 
                       !c.IsDeleted)
            .CountAsync();
    }

    public async Task<IEnumerable<ChatSession>> GetActiveSessionsAsync(int userId)
    {
        return await _context.ChatSessions
            .Include(c => c.Provider)
            .Include(c => c.Messages)
            .Where(c => c.UserId == userId && 
                       c.Status == ChatSession.ChatStatus.Active && 
                       !c.IsDeleted)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<bool> CanStartSessionAsync(int userId, Guid subscriptionId)
    {
        // Get subscription and check if active
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.Status == Subscription.SubscriptionStatuses.Active);

        if (subscription == null) return false;

        // Get service constraint for chat
        var constraint = await GetServiceConstraintAsync(subscription.SubscriptionPlanId, "InstantChat");
        if (constraint == null) return false;

        // Check session count limit
        if (constraint.MaxSessionsPerMonth > 0)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var monthlySessions = await GetMonthlySessionCountAsync(subscriptionId, startOfMonth, DateTime.UtcNow);
            if (monthlySessions >= constraint.MaxSessionsPerMonth)
                return false;
        }

        // Check concurrent sessions limit
        if (constraint.MaxConcurrentSessions > 0)
        {
            var activeSessions = await GetActiveSessionCountAsync(userId);
            if (activeSessions >= constraint.MaxConcurrentSessions)
                return false;
        }

        // Check time-based limits
        if (constraint.TotalMinutesPerMonth.HasValue)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var monthlyMinutes = await GetMonthlyTotalMinutesAsync(subscriptionId, startOfMonth, DateTime.UtcNow);
            if (monthlyMinutes >= constraint.TotalMinutesPerMonth.Value)
                return false;
        }

        return true;
    }

    public async Task<bool> CanSendMessageAsync(Guid sessionId, string message)
    {
        var session = await GetByIdAsync(sessionId);
        if (session == null || session.Status != ChatSession.ChatStatus.Active)
            return false;

        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.Id == session.SubscriptionId);

        if (subscription == null) return false;

        var constraint = await GetServiceConstraintAsync(subscription.SubscriptionPlanId, "InstantChat");
        if (constraint == null) return false;

        // Check message length limit
        if (message.Length > constraint.MaxMessageLength)
            return false;

        return true;
    }

    public async Task<bool> CanUploadFileAsync(Guid sessionId, long fileSize)
    {
        var session = await GetByIdAsync(sessionId);
        if (session == null || session.Status != ChatSession.ChatStatus.Active)
            return false;

        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.Id == session.SubscriptionId);

        if (subscription == null) return false;

        var constraint = await GetServiceConstraintAsync(subscription.SubscriptionPlanId, "InstantChat");
        if (constraint == null || !constraint.AllowFileSharing)
            return false;

        // Check file size limit (10MB default)
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        return fileSize <= maxFileSize;
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        message.CreatedDate = DateTime.UtcNow;
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(Guid sessionId)
    {
        return await _context.ChatMessages
            .Where(m => m.SessionId == sessionId && !m.IsDeleted)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<bool> MarkMessageAsReadAsync(Guid messageId, int userId)
    {
        var message = await _context.ChatMessages.FindAsync(messageId);
        if (message == null) return false;

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        message.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ChatAttachment> AddAttachmentAsync(ChatAttachment attachment)
    {
        attachment.CreatedDate = DateTime.UtcNow;
        _context.ChatAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<IEnumerable<ChatAttachment>> GetSessionAttachmentsAsync(Guid sessionId)
    {
        return await _context.ChatAttachments
            .Where(a => a.SessionId == sessionId && !a.IsDeleted)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatSession>> GetSessionsByDateRangeAsync(Guid subscriptionId, DateTime startDate, DateTime endDate)
    {
        return await _context.ChatSessions
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Messages)
            .Where(c => c.SubscriptionId == subscriptionId && 
                       c.StartTime >= startDate && 
                       c.StartTime <= endDate && 
                       !c.IsDeleted)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<ChatUsageStatistics> GetUsageStatisticsAsync(Guid subscriptionId, DateTime startDate, DateTime endDate)
    {
        var sessions = await GetSessionsByDateRangeAsync(subscriptionId, startDate, endDate);
        
        if (!sessions.Any())
            return new ChatUsageStatistics();

        var totalSessions = sessions.Count();
        var totalMinutes = sessions.Sum(s => s.DurationMinutes);
        var totalMessages = sessions.Sum(s => s.MessageCount);
        var totalAttachments = sessions.Sum(s => s.Attachments.Count);

        return new ChatUsageStatistics
        {
            TotalSessions = totalSessions,
            TotalMinutes = totalMinutes,
            TotalMessages = totalMessages,
            TotalAttachments = totalAttachments,
            AverageSessionDuration = totalSessions > 0 ? (double)totalMinutes / totalSessions : 0,
            AverageMessagesPerSession = totalSessions > 0 ? (double)totalMessages / totalSessions : 0
        };
    }

    private async Task<ServiceConstraint?> GetServiceConstraintAsync(Guid planId, string serviceName)
    {
        return await _context.ServiceConstraints
            .FirstOrDefaultAsync(sc => sc.SubscriptionPlanId == planId && 
                                     sc.ServiceName == serviceName && 
                                     !sc.IsDisabled && 
                                     !sc.IsDeleted);
    }
} 