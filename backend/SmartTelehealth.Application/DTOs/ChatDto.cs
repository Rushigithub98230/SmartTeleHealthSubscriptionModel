public class CreateMessageDto
{
    public string ChatRoomId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public bool IsEncrypted { get; set; } = false;
    public string? ReplyToMessageId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string? AttachmentType { get; set; }
    public long? AttachmentSize { get; set; }
} 