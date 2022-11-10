using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

public abstract class AbstractFormPage : PageModel
{
    private const string PageHistorySessionKey = "PageHistory";
    public const string PageModelSessionKey = "PageModel";

    /// <summary>
    /// Removes the page model stored in session
    /// </summary>
    protected void ClearPageModel()
    {
        HttpContext.Session.Remove(PageModelSessionKey);
    }

    /// <summary>
    /// Removes the page history stored in session
    /// </summary>
    protected void ClearPageHistory()
    {
        HttpContext.Session.Remove(PageHistorySessionKey);
    }
}
