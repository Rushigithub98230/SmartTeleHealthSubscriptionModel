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

    public override async Task<Notification> CreateAsync(Notification notification)
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

    public override async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _dbContext.Notifications.OrderByDescending(n => n.CreatedDate).ToListAsync();
    }

    public override async Task<Notification?> GetByIdAsync(object id)
    {
        if (id is not Guid notificationId)
            return null;

        return await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    public override async Task<Notification> UpdateAsync(Notification notification)
    {
        return await base.UpdateAsync(notification);
    }

    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid notificationId)
            return false;

        var notification = await _dbContext.Notifications.FindAsync(notificationId);
        if (notification == null)
            return false;
        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid notificationId)
            return false;

        return await _dbContext.Notifications.AnyAsync(n => n.Id == notificationId);
    }
} 