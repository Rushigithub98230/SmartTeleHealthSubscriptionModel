using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class VideoCallRepository : IVideoCallRepository
{
    private readonly ApplicationDbContext _context;

    public VideoCallRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VideoCall?> GetByIdAsync(Guid id)
    {
        return await _context.VideoCalls
            .Include(vc => vc.Participants)
            .Include(vc => vc.Events)
            .FirstOrDefaultAsync(vc => vc.Id == id);
    }

    public async Task<IEnumerable<VideoCall>> GetByUserIdAsync(int userId)
    {
        return await _context.VideoCalls
            .Include(vc => vc.Participants)
            .Where(vc => vc.Participants.Any(p => p.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<VideoCall>> GetAllAsync()
    {
        return await _context.VideoCalls
            .Include(vc => vc.Participants)
            .Include(vc => vc.Events)
            .ToListAsync();
    }

    public async Task<VideoCall> CreateAsync(VideoCall videoCall)
    {
        _context.VideoCalls.Add(videoCall);
        await _context.SaveChangesAsync();
        return videoCall;
    }

    public async Task<VideoCall> UpdateAsync(VideoCall videoCall)
    {
        videoCall.UpdatedDate = DateTime.UtcNow;
        _context.VideoCalls.Update(videoCall);
        await _context.SaveChangesAsync();
        return videoCall;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var videoCall = await _context.VideoCalls.FindAsync(id);
        if (videoCall == null)
            return false;

        videoCall.IsDeleted = true;
        videoCall.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
} 