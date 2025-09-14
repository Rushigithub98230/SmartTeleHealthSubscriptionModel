using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentParticipantRepository : IRepositoryBase<AppointmentParticipant>
{
    // Basic CRUD methods are inherited from IRepositoryBase<AppointmentParticipant>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Specialized methods for AppointmentParticipant entity
    Task<IEnumerable<AppointmentParticipant>> GetByAppointmentAsync(Guid appointmentId);
    Task<IEnumerable<AppointmentParticipant>> GetByUserAsync(int userId);
    Task<AppointmentParticipant?> GetByAppointmentAndUserAsync(Guid appointmentId, int? userId);
    Task<Guid> GetStatusIdByNameAsync(string name);
    Task<AppointmentParticipant?> FindByAppointmentAndUserOrEmailAsync(Guid appointmentId, int? userId, string? email);
    Task<AppointmentParticipant?> GetByUserAndAppointmentAsync(int userId, Guid appointmentId);
} 