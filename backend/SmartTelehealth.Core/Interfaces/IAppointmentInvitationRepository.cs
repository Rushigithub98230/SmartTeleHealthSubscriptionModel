using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentInvitationRepository : IRepositoryBase<AppointmentInvitation>
{
    // Basic CRUD methods are inherited from IRepositoryBase<AppointmentInvitation>
    
    Task<IEnumerable<AppointmentInvitation>> GetByAppointmentAsync(Guid appointmentId);
    Task<IEnumerable<AppointmentInvitation>> GetByInviteeAsync(int inviteeId);
    Task<AppointmentInvitation?> GetByTokenAsync(string token);
    Task<Guid> GetStatusIdByNameAsync(string name);
} 