using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IVideoCallRepository : IRepositoryBase<VideoCall>
{
    // Basic CRUD methods are inherited from IRepositoryBase<VideoCall>
    
    Task<IEnumerable<VideoCall>> GetByUserIdAsync(int userId);
} 