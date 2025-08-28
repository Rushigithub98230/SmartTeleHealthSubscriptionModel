public class BookAppointmentDto
{
    public string PatientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string? SubscriptionId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public Guid ConsultationModeId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string ReasonForVisit { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? PatientNotes { get; set; }
    public bool IsUrgent { get; set; } = false;
} 