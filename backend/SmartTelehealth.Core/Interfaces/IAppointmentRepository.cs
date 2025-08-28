using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByProviderAsync(int providerId);
    Task<IEnumerable<Appointment>> GetByStatusAsync(Guid appointmentStatusId);
    Task<IEnumerable<Appointment>> GetUpcomingAsync();
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetCountByStatusAsync(Guid appointmentStatusId);
    Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    Task<Guid> GetStatusIdByNameAsync(string statusName);
} 