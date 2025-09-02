namespace SmartTelehealth.Application.DTOs;

public class CreateProviderFeeDto
{
    public int ProviderId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal ProposedFee { get; set; }
    public string? ProviderNotes { get; set; }
}

public class UpdateProviderFeeDto
{
    public decimal ProposedFee { get; set; }
    public string? ProviderNotes { get; set; }
}

public class ProviderFeeDto
{
    public Guid Id { get; set; }
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal ProposedFee { get; set; }
    public decimal ApprovedFee { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminRemarks { get; set; }
    public string? ProviderNotes { get; set; }
    public DateTime? ProposedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUserName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class ReviewProviderFeeDto
{
    public string Status { get; set; } = string.Empty;
    public decimal? ApprovedFee { get; set; }
    public string? AdminRemarks { get; set; }
    public int? ReviewedByUserId { get; set; }
}

public class CategoryFeeRangeDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal MinimumFee { get; set; }
    public decimal MaximumFee { get; set; }
    public decimal PlatformCommission { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class CreateCategoryFeeRangeDto
{
    public Guid CategoryId { get; set; }
    public decimal MinimumFee { get; set; }
    public decimal MaximumFee { get; set; }
    public decimal PlatformCommission { get; set; }
    public string? Description { get; set; }
}

public class UpdateCategoryFeeRangeDto
{
    public decimal MinimumFee { get; set; }
    public decimal MaximumFee { get; set; }
    public decimal PlatformCommission { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
} 