using SmartTelehealth.Application.DTOs;

public class UpdateProviderScheduleDto
{
    public string ProviderId { get; set; } = string.Empty;
    public List<ProviderAvailabilityDto> Availability { get; set; } = new();
    public bool IsAvailable { get; set; } = true;
    public string? Notes { get; set; }
    public List<WeeklyScheduleDto> WeeklySchedule { get; set; } = new();
    public List<DateTime> AvailableDates { get; set; } = new();
    public List<DateTime> UnavailableDates { get; set; } = new();
    public int DefaultDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
} 