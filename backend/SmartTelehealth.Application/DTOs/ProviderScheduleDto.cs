public class ProviderScheduleDto
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    public List<TimeSlotDto> BookedSlots { get; set; } = new();
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public List<WeeklyScheduleDto> WeeklySchedule { get; set; } = new();
    public List<DateTime> AvailableDates { get; set; } = new();
    public List<DateTime> UnavailableDates { get; set; } = new();
    public int DefaultDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
} 