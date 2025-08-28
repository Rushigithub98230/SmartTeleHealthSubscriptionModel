using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentPaymentLogRepository
{
    Task<AppointmentPaymentLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<AppointmentPaymentLog>> GetAllAsync();
    Task<AppointmentPaymentLog> CreateAsync(AppointmentPaymentLog paymentLog);
    Task<AppointmentPaymentLog> UpdateAsync(AppointmentPaymentLog paymentLog);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<AppointmentPaymentLog>> GetByAppointmentAsync(Guid appointmentId);
    Task<AppointmentPaymentLog?> GetLatestByAppointmentAsync(Guid appointmentId);
    Task<IEnumerable<AppointmentPaymentLog>> GetByPaymentStatusAsync(Guid paymentStatusId);
    Task<IEnumerable<AppointmentPaymentLog>> GetByRefundStatusAsync(Guid refundStatusId);
    Task<AppointmentPaymentLog?> FindByPaymentIntentIdAsync(string paymentIntentId);
    Task<AppointmentPaymentLog?> FindByRefundIdAsync(string refundId);
    Task<Guid> GetStatusIdByNameAsync(string name);
} 