namespace SmartTelehealth.Application.DTOs;

public class StorageInfoDto
{
    public long TotalSpace { get; set; }
    public long UsedSpace { get; set; }
    public long AvailableSpace { get; set; }
    public int FileCount { get; set; }
    public int DirectoryCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
} 