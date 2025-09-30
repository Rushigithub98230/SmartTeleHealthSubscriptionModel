using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentRepository : IRepositoryBase<Appointment>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Appointment>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Custom methods with business logic
    Task<Appointment?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Appointment>> GetAllWithDetailsAsync();
    Task<Appointment> CreateAppointmentAsync(Appointment appointment);
    Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
    Task<bool> DeleteAppointmentAsync(Guid id);
    Task<bool> ExistsAppointmentAsync(Guid id);

    // Specialized methods for Appointment entity
    Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByProviderAsync(int providerId);
    Task<IEnumerable<Appointment>> GetByStatusAsync(Guid appointmentStatusId);
    Task<IEnumerable<Appointment>> GetUpcomingAsync();
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetCountByStatusAsync(Guid appointmentStatusId);
    Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    Task<Guid> GetStatusIdByNameAsync(string statusName);
    
    // Additional specialized methods
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTime fromDate);
    Task<IEnumerable<Appointment>> GetOverdueAppointmentsAsync();
    
    // Legacy methods for backward compatibility
    Task<Appointment> AddAsync(Appointment appointment);
    Task<int> GetCountAsync();
} 