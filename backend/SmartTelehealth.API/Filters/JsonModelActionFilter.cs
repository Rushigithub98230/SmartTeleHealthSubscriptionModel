using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Filters;

public class JsonModelActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // No action needed before execution
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is JsonModel jsonModel)
        {
            // Set the HTTP status code based on JsonModel.StatusCode
            context.HttpContext.Response.StatusCode = jsonModel.StatusCode;
            
            // Ensure the response is returned as-is without additional wrapping
            context.Result = new ObjectResult(jsonModel)
            {
                StatusCode = null // Let the filter handle the status code
            };
        }
    }
}
