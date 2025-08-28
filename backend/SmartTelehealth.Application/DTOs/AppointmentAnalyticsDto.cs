public class AppointmentAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageAppointmentDuration { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
    public Dictionary<string, int> AppointmentsByCategory { get; set; } = new();
} 