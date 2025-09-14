using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.DTOs;

/// <summary>
/// DTO for comprehensive filtering of subscription plans with all possible filter parameters.
/// This DTO consolidates all filtering options into a single, reusable structure.
/// </summary>
public class SubscriptionPlanFilterDto
{
    #region Pagination Parameters
    
    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Page size must be between 1 and 1000")]
    public int PageSize { get; set; } = 50;
    
    #endregion
    
    #region Search Parameters
    
    /// <summary>
    /// Search term for filtering plans by name, description, or short description
    /// </summary>
    public string? SearchTerm { get; set; }
    
    #endregion
    
    #region Category and Classification Filters
    
    /// <summary>
    /// Filter by specific category ID
    /// </summary>
    public Guid? CategoryId { get; set; }
    
    /// <summary>
    /// Filter by category name (partial match)
    /// </summary>
    public string? CategoryName { get; set; }
    
    #endregion
    
    #region Status and Feature Filters
    
    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Filter by featured status
    /// </summary>
    public bool? IsFeatured { get; set; }
    
    /// <summary>
    /// Filter by most popular status
    /// </summary>
    public bool? IsMostPopular { get; set; }
    
    /// <summary>
    /// Filter by trending status
    /// </summary>
    public bool? IsTrending { get; set; }
    
    /// <summary>
    /// Filter by trial allowed status
    /// </summary>
    public bool? IsTrialAllowed { get; set; }
    
    #endregion
    
    #region Pricing Filters
    
    /// <summary>
    /// Minimum price filter
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Minimum price must be non-negative")]
    public decimal? MinPrice { get; set; }
    
    /// <summary>
    /// Maximum price filter
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Maximum price must be non-negative")]
    public decimal? MaxPrice { get; set; }
    
    /// <summary>
    /// Filter by specific price
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal? ExactPrice { get; set; }
    
    /// <summary>
    /// Filter by currency ID
    /// </summary>
    public Guid? CurrencyId { get; set; }
    
    #endregion
    
    #region Billing Cycle Filters
    
    /// <summary>
    /// Filter by billing cycle ID
    /// </summary>
    public Guid? BillingCycleId { get; set; }
    
    /// <summary>
    /// Filter by billing cycle name (e.g., "monthly", "quarterly", "annual")
    /// </summary>
    public string? BillingCycleName { get; set; }
    
    #endregion
    
    #region Date Range Filters
    
    /// <summary>
    /// Filter by creation date range - start date
    /// </summary>
    public DateTime? CreatedDateFrom { get; set; }
    
    /// <summary>
    /// Filter by creation date range - end date
    /// </summary>
    public DateTime? CreatedDateTo { get; set; }
    
    /// <summary>
    /// Filter by update date range - start date
    /// </summary>
    public DateTime? UpdatedDateFrom { get; set; }
    
    /// <summary>
    /// Filter by update date range - end date
    /// </summary>
    public DateTime? UpdatedDateTo { get; set; }
    
    /// <summary>
    /// Filter by effective date range - start date
    /// </summary>
    public DateTime? EffectiveDateFrom { get; set; }
    
    /// <summary>
    /// Filter by effective date range - end date
    /// </summary>
    public DateTime? EffectiveDateTo { get; set; }
    
    #endregion
    
    #region Trial Period Filters
    
    /// <summary>
    /// Minimum trial duration in days
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Trial duration must be non-negative")]
    public int? MinTrialDuration { get; set; }
    
    /// <summary>
    /// Maximum trial duration in days
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Trial duration must be non-negative")]
    public int? MaxTrialDuration { get; set; }
    
    #endregion
    
    #region Display Order Filters
    
    /// <summary>
    /// Minimum display order
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be non-negative")]
    public int? MinDisplayOrder { get; set; }
    
    /// <summary>
    /// Maximum display order
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be non-negative")]
    public int? MaxDisplayOrder { get; set; }
    
    #endregion
    
    #region Stripe Integration Filters
    
    /// <summary>
    /// Filter by Stripe product ID
    /// </summary>
    public string? StripeProductId { get; set; }
    
    /// <summary>
    /// Filter by Stripe integration status (has Stripe product or not)
    /// </summary>
    public bool? HasStripeIntegration { get; set; }
    
    #endregion
    
    #region Sorting Parameters
    
    /// <summary>
    /// Column name for sorting (e.g., "Name", "Price", "CreatedDate", "DisplayOrder")
    /// </summary>
    public string SortColumn { get; set; } = "DisplayOrder";
    
    /// <summary>
    /// Sort order (asc/desc)
    /// </summary>
    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort order must be 'asc' or 'desc'")]
    public string SortOrder { get; set; } = "asc";
    
    #endregion
    
    #region Additional Filters
    
    /// <summary>
    /// Filter by specific plan IDs
    /// </summary>
    public List<Guid>? PlanIds { get; set; }
    
    /// <summary>
    /// Exclude specific plan IDs
    /// </summary>
    public List<Guid>? ExcludePlanIds { get; set; }
    
    /// <summary>
    /// Filter by plans that have active subscriptions
    /// </summary>
    public bool? HasActiveSubscriptions { get; set; }
    
    /// <summary>
    /// Filter by plans that have any subscriptions (active or inactive)
    /// </summary>
    public bool? HasSubscriptions { get; set; }
    
    #endregion
    
    #region Validation Methods
    
    /// <summary>
    /// Validates the filter parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        // Validate date ranges
        if (CreatedDateFrom.HasValue && CreatedDateTo.HasValue && CreatedDateFrom > CreatedDateTo)
            return false;
            
        if (UpdatedDateFrom.HasValue && UpdatedDateTo.HasValue && UpdatedDateFrom > UpdatedDateTo)
            return false;
            
        if (EffectiveDateFrom.HasValue && EffectiveDateTo.HasValue && EffectiveDateFrom > EffectiveDateTo)
            return false;
        
        // Validate price ranges
        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            return false;
            
        if (MinTrialDuration.HasValue && MaxTrialDuration.HasValue && MinTrialDuration > MaxTrialDuration)
            return false;
            
        if (MinDisplayOrder.HasValue && MaxDisplayOrder.HasValue && MinDisplayOrder > MaxDisplayOrder)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Gets validation errors
    /// </summary>
    /// <returns>List of validation error messages</returns>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (CreatedDateFrom.HasValue && CreatedDateTo.HasValue && CreatedDateFrom > CreatedDateTo)
            errors.Add("Created date from cannot be greater than created date to");
            
        if (UpdatedDateFrom.HasValue && UpdatedDateTo.HasValue && UpdatedDateFrom > UpdatedDateTo)
            errors.Add("Updated date from cannot be greater than updated date to");
            
        if (EffectiveDateFrom.HasValue && EffectiveDateTo.HasValue && EffectiveDateFrom > EffectiveDateTo)
            errors.Add("Effective date from cannot be greater than effective date to");
        
        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            errors.Add("Minimum price cannot be greater than maximum price");
            
        if (MinTrialDuration.HasValue && MaxTrialDuration.HasValue && MinTrialDuration > MaxTrialDuration)
            errors.Add("Minimum trial duration cannot be greater than maximum trial duration");
            
        if (MinDisplayOrder.HasValue && MaxDisplayOrder.HasValue && MinDisplayOrder > MaxDisplayOrder)
            errors.Add("Minimum display order cannot be greater than maximum display order");
        
        return errors;
    }
    
    #endregion
}
