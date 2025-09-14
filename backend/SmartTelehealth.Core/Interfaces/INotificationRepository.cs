using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface INotificationRepository : IRepositoryBase<Notification>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Notification>
    
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task MarkAsReadAsync(Guid notificationId);
} 