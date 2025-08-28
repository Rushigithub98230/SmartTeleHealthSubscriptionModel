using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task MarkAsReadAsync(Guid notificationId);
    Task<IEnumerable<Notification>> GetAllAsync();
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(Guid id);
} 