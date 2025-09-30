using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class NotificationRepository : RepositoryBase<Notification>, INotificationRepository
{
    private readonly ApplicationDbContext _dbContext;
    public NotificationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Notification> CreateNotificationAsync(Notification notification)
    {
        return await base.CreateAsync(notification);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _dbContext.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Notification>> GetAllWithDetailsAsync()
    {
        return await _dbContext.Notifications.OrderByDescending(n => n.CreatedDate).ToListAsync();
    }

    public async Task<Notification?> GetByIdWithDetailsAsync(Guid notificationId)
    {
        return await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    public async Task<Notification> UpdateNotificationAsync(Notification notification)
    {
        return await base.UpdateAsync(notification);
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId)
    {
        var notification = await _dbContext.Notifications.FindAsync(notificationId);
        if (notification == null)
            return false;
        
        notification.IsActive = false;
        notification.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsNotificationAsync(Guid notificationId)
    {
        return await _dbContext.Notifications.AnyAsync(n => n.Id == notificationId);
    }
} 