using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _dbContext;
    public NotificationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
        return notification;
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

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _dbContext.Notifications.OrderByDescending(n => n.CreatedDate).ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _dbContext.Notifications.Update(notification);
        await _dbContext.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var notification = await _dbContext.Notifications.FindAsync(id);
        if (notification == null)
            return false;
        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync();
        return true;
    }
} 