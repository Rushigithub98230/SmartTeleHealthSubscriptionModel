using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IChatSessionRepository
{
    // Basic CRUD operations
    Task<ChatSession?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChatSession>> GetByUserIdAsync(int userId);
    Task<IEnumerable<ChatSession>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<ChatSession>> GetByProviderIdAsync(int providerId);
    Task<ChatSession> CreateAsync(ChatSession session);
    Task<ChatSession> UpdateAsync(ChatSession session);
    Task<bool> DeleteAsync(Guid id);

    // Usage tracking methods
    Task<int> GetMonthlySessionCountAsync(Guid subscriptionId, DateTime startDate, DateTime endDate);
    Task<int> GetMonthlyTotalMinutesAsync(Guid subscriptionId, DateTime startDate, DateTime endDate);
    Task<int> GetActiveSessionCountAsync(int userId);
    Task<IEnumerable<ChatSession>> GetActiveSessionsAsync(int userId);

    // Constraint checking methods
    Task<bool> CanStartSessionAsync(int userId, Guid subscriptionId);
    Task<bool> CanSendMessageAsync(Guid sessionId, string message);
    Task<bool> CanUploadFileAsync(Guid sessionId, long fileSize);

    // Message operations
    Task<ChatMessage> AddMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(Guid sessionId);
    Task<bool> MarkMessageAsReadAsync(Guid messageId, int userId);

    // Attachment operations
    Task<ChatAttachment> AddAttachmentAsync(ChatAttachment attachment);
    Task<IEnumerable<ChatAttachment>> GetSessionAttachmentsAsync(Guid sessionId);

    // Analytics and reporting
    Task<IEnumerable<ChatSession>> GetSessionsByDateRangeAsync(Guid subscriptionId, DateTime startDate, DateTime endDate);
    Task<ChatUsageStatistics> GetUsageStatisticsAsync(Guid subscriptionId, DateTime startDate, DateTime endDate);
}

public class ChatUsageStatistics
{
    public int TotalSessions { get; set; }
    public int TotalMinutes { get; set; }
    public int TotalMessages { get; set; }
    public int TotalAttachments { get; set; }
    public double AverageSessionDuration { get; set; }
    public double AverageMessagesPerSession { get; set; }
} 