using Microsoft.AspNetCore.Mvc.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Filters;

/// <summary>
/// Used on page models where the page history should be tracked.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PageHistoryAttribute : ResultFilterAttribute
{
    public const string PageHistorySessionKey = "PageHistory";

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context?.HttpContext?.Request?.Method is not null && context.HttpContext.Request.Method.ToUpper() == "GET")
        {
            var pageHistoryModel = context.HttpContext.Session.GetOrDefault<PageHistoryModel>(PageHistorySessionKey) ?? new PageHistoryModel();
            string currentPagePath = context.HttpContext.Request.Path.ToString();

            if (pageHistoryModel.PageHistory.Any() && pageHistoryModel.PageHistory.Last() == currentPagePath)
            {
                // User has gone back a step - remove page from history
                pageHistoryModel.PageHistory.Remove(currentPagePath);
            }
            else if (!string.IsNullOrEmpty(pageHistoryModel.CurrentPagePath) && pageHistoryModel.CurrentPagePath != currentPagePath)
            {
                // User has gone forward a step - add previous page to history
                pageHistoryModel.PageHistory.Add(pageHistoryModel.CurrentPagePath);
            }

            pageHistoryModel.CurrentPagePath = currentPagePath;
            context.HttpContext.Session.Put(PageHistorySessionKey, pageHistoryModel);
        }
    }
}
