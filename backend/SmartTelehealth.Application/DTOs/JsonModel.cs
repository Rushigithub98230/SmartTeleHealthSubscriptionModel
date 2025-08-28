using System;

namespace SmartTelehealth.Application.DTOs
{
    public class JsonModel
    {
        public JsonModel()
        {

        }
       
        public JsonModel(object responseData, string message, int statusCode)
        {
            data = responseData;
            Message = message;
            StatusCode = statusCode;
        }

        public object data { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public Meta meta { get; set; }
    }

    public class Meta
    {
        public Meta()
        {

        }
        public Meta(dynamic T, dynamic searchFilterModel)
        {
            try
            {
                TotalRecords = T != null && T.Count > 0 ? T[0].TotalRecords : 0;
                CurrentPage = searchFilterModel.pageNumber;
                PageSize = searchFilterModel.pageSize;
                DefaultPageSize = searchFilterModel.pageSize;
                TotalPages = Math.Ceiling(Convert.ToDecimal((T != null && T.Count > 0 ? T[0].TotalRecords : 0) / searchFilterModel.pageSize));
            }
            catch (Exception)
            {
            }
        }
        public decimal TotalPages { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int DefaultPageSize { get; set; }
        public decimal TotalRecords { get; set; }
    }
}

// Bulk operation DTOs
public class BulkActionRequestDto
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public int? AdditionalDays { get; set; }
}

public class BulkActionResultDto
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkActionResultDto> Results { get; set; } = new List<BulkActionResultDto>();
}
