using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentInvitationRepository
{
    Task<AppointmentInvitation?> GetByIdAsync(Guid id);
    Task<IEnumerable<AppointmentInvitation>> GetAllAsync();
    Task<AppointmentInvitation> CreateAsync(AppointmentInvitation invitation);
    Task<AppointmentInvitation> UpdateAsync(AppointmentInvitation invitation);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<AppointmentInvitation>> GetByAppointmentAsync(Guid appointmentId);
    Task<IEnumerable<AppointmentInvitation>> GetByInviteeAsync(int inviteeId);
    Task<AppointmentInvitation?> GetByTokenAsync(string token);
    Task<Guid> GetStatusIdByNameAsync(string name);
} 