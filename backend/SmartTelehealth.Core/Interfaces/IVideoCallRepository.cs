using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IVideoCallRepository
{
    Task<VideoCall?> GetByIdAsync(Guid id);
    Task<IEnumerable<VideoCall>> GetByUserIdAsync(int userId);
    Task<IEnumerable<VideoCall>> GetAllAsync();
    Task<VideoCall> CreateAsync(VideoCall videoCall);
    Task<VideoCall> UpdateAsync(VideoCall videoCall);
    Task<bool> DeleteAsync(Guid id);
} 