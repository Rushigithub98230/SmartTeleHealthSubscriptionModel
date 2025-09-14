using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentPaymentLogRepository : IRepositoryBase<AppointmentPaymentLog>
{
    // Basic CRUD methods are inherited from IRepositoryBase<AppointmentPaymentLog>
    
    Task<IEnumerable<AppointmentPaymentLog>> GetByAppointmentAsync(Guid appointmentId);
    Task<AppointmentPaymentLog?> GetLatestByAppointmentAsync(Guid appointmentId);
    Task<IEnumerable<AppointmentPaymentLog>> GetByPaymentStatusAsync(Guid paymentStatusId);
    Task<IEnumerable<AppointmentPaymentLog>> GetByRefundStatusAsync(Guid refundStatusId);
    Task<AppointmentPaymentLog?> FindByPaymentIntentIdAsync(string paymentIntentId);
    Task<AppointmentPaymentLog?> FindByRefundIdAsync(string refundId);
    Task<Guid> GetStatusIdByNameAsync(string name);
} 