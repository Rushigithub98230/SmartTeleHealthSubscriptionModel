namespace SmartTelehealth.Core.DTOs
{
    public class TokenModel
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
