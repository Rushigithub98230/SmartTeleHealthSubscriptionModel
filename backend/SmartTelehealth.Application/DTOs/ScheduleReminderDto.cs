public class ScheduleReminderDto
{
    public Guid ReminderTypeId { get; set; }
    public Guid ReminderTimingId { get; set; }
    public string? Message { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
} 