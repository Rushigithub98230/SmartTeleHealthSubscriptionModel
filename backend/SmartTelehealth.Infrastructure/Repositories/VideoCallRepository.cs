using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class VideoCallRepository : RepositoryBase<VideoCall>, IVideoCallRepository
{
    private readonly ApplicationDbContext _context;

    public VideoCallRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a video call by its unique identifier with related entities
    /// </summary>
    public override async Task<VideoCall?> GetByIdAsync(object id)
    {
        if (id is not Guid callId)
            return null;

        return await _context.VideoCalls
            .Include(vc => vc.Participants)
            .Include(vc => vc.Events)
            .FirstOrDefaultAsync(vc => vc.Id == callId);
    }

    public async Task<IEnumerable<VideoCall>> GetByUserIdAsync(int userId)
    {
        return await _context.VideoCalls
            .Include(vc => vc.Participants)
            .Where(vc => vc.Participants.Any(p => p.UserId == userId))
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all video calls with related entities
    /// </summary>
    public override async Task<IEnumerable<VideoCall>> GetAllAsync()
    {
        return await _context.VideoCalls
            .Include(vc => vc.Participants)
            .Include(vc => vc.Events)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new video call
    /// </summary>
    public override async Task<VideoCall> CreateAsync(VideoCall videoCall)
    {
        videoCall.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(videoCall);
    }

    /// <summary>
    /// Updates an existing video call
    /// </summary>
    public override async Task<VideoCall> UpdateAsync(VideoCall videoCall)
    {
        videoCall.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(videoCall);
    }

    /// <summary>
    /// Deletes a video call by its unique identifier (soft delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid callId)
            return false;

        var videoCall = await _context.VideoCalls.FindAsync(callId);
        if (videoCall == null)
            return false;

        videoCall.IsDeleted = true;
        videoCall.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a video call exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid callId)
            return false;

        return await _context.VideoCalls
            .AnyAsync(vc => vc.Id == callId && !vc.IsDeleted);
    }
} 