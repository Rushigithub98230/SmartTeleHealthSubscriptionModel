public class WeeklyScheduleDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public string DayName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DefaultDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
} 