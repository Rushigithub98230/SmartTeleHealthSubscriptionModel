namespace SmartTelehealth.Application.DTOs;

public class FileInfoDto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
} 