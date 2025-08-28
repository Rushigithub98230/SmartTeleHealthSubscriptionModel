public class UploadDocumentDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int DocumentTypeId { get; set; }
    public string? Description { get; set; }
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
} 