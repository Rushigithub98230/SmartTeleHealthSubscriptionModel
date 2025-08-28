namespace SmartTelehealth.Application.DTOs
{
    public class FileUploadDto
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string? ContentType { get; set; }
        public string? Directory { get; set; }
    }
} 