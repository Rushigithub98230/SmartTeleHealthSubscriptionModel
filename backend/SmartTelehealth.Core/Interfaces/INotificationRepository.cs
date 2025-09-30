using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface INotificationRepository : IRepositoryBase<Notification>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Notification>
    
    // Custom methods with business logic
    Task<Notification> CreateNotificationAsync(Notification notification);
    Task<IEnumerable<Notification>> GetAllWithDetailsAsync();
    Task<Notification?> GetByIdWithDetailsAsync(Guid notificationId);
    Task<Notification> UpdateNotificationAsync(Notification notification);
    Task<bool> DeleteNotificationAsync(Guid notificationId);
    Task<bool> ExistsNotificationAsync(Guid notificationId);
    
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task MarkAsReadAsync(Guid notificationId);
} 