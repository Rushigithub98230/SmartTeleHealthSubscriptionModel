using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentParticipantRepository
{
    Task<AppointmentParticipant?> GetByIdAsync(Guid id);
    Task<IEnumerable<AppointmentParticipant>> GetAllAsync();
    Task<AppointmentParticipant> CreateAsync(AppointmentParticipant participant);
    Task<AppointmentParticipant> UpdateAsync(AppointmentParticipant participant);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<AppointmentParticipant>> GetByAppointmentAsync(Guid appointmentId);
    Task<IEnumerable<AppointmentParticipant>> GetByUserAsync(int userId);
    Task<AppointmentParticipant?> GetByAppointmentAndUserAsync(Guid appointmentId, int? userId);
    Task<Guid> GetStatusIdByNameAsync(string name);
    Task<AppointmentParticipant?> FindByAppointmentAndUserOrEmailAsync(Guid appointmentId, int? userId, string? email);
    Task<AppointmentParticipant?> GetByUserAndAppointmentAsync(int userId, Guid appointmentId);
} 