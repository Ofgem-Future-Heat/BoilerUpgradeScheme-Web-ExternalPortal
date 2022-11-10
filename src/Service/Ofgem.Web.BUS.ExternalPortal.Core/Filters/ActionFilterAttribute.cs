using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Filters;

public class CustomActionFilterAttribute : ActionFilterAttribute
{
    private readonly ILogger _logger;
    public CustomActionFilterAttribute(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("CustomActionFilter");
    }
    
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        _logger.LogWarning("Inside OnResultExecuting method...");
        if (!context.ModelState.IsValid && context.Controller is PageModel controller)
        {
            controller.ViewData["ErrorTitle"] = "Error:";
        }

        base.OnResultExecuting(context);
    }
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        _logger.LogWarning("Inside OnResultExecuted method...");
        if (!context.ModelState.IsValid && context.Controller is PageModel controller)
        {
            controller.ViewData["ErrorTitle"] = "Error:"; 
        }

        base.OnResultExecuted(context);
    }
}