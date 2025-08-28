public class DayScheduleDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MaxAppointments { get; set; }
    public string? Notes { get; set; }
} 