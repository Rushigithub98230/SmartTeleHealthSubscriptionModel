namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Detailed pricing breakdown for transparency.
/// Used for displaying how plan prices are calculated from privileges and commission.
/// </summary>
public class PricingBreakdown
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public bool IsAutoCalculated { get; set; }
    public List<PrivilegeBreakdownItem> PrivilegeBreakdown { get; set; } = new();
    public decimal PrivilegesTotalCost { get; set; }
    public decimal CommissionPercent { get; set; }
    public decimal CommissionAmount { get; set; }
    public bool IsFixedCommission { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? PromotionalDiscountPercent { get; set; }
    public decimal? PromotionalDiscountAmount { get; set; }
    public decimal? BillingDiscountPercent { get; set; }
    public decimal? BillingDiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal? ManualPrice { get; set; }
}

/// <summary>
/// Individual privilege contribution to plan price.
/// Shows how each privilege contributes to the overall plan cost.
/// </summary>
public class PrivilegeBreakdownItem
{
    public string PrivilegeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitBaseCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal OverageUnitCost { get; set; }
}

