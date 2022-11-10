using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Cancellation;

[PageHistory]
public class CancelModel : AbstractFormPage
{
    [Required(ErrorMessage = "Tell us whether you want to cancel this application")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<CancelModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public CancelModel(ILogger<CancelModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// OnPost - gets the user response as to if they wish to cancel the application or not!
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnPost()
    {
        _logger.LogDebug("CancelModel -> OnPost");

        if (!ModelState.IsValid || QuestionResponse == null)
        {
            return Page();
        }

        /* Where did I come from? */
        var pageHistoryModel = HttpContext.Session.GetOrDefault<PageHistoryModel>(PageHistoryAttribute.PageHistorySessionKey);
        var previousPageUrl = pageHistoryModel?.PageHistory.LastOrDefault() ?? @Routes.Pages.Path.CD155a;

        if (QuestionResponse == true)
        {
            ClearPageModel();
            return RedirectToPage(@Routes.Pages.Path.CD155a);
        }
        else
        {
            var prevPage = Routes.RouteList.Where(x => previousPageUrl.Contains(x.route)).Select(r => r.page).FirstOrDefault();

            return RedirectToPage(prevPage);
        }
    }
}
