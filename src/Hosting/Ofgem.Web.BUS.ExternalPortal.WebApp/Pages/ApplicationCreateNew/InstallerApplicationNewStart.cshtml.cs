using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew;

public class InstallerApplicationNewStartModel : AbstractFormPage
{
    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<InstallerApplicationNewStartModel> _logger;

    /// <summary>
    /// Session handler for user interactions.
    /// </summary>
    private readonly SessionService _session;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <param name="session">Session for local user retained values.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public InstallerApplicationNewStartModel(ILogger<InstallerApplicationNewStartModel> logger, SessionService session)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _session = session ?? throw new ArgumentNullException(nameof(session));

    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnGet()
    {
        _logger.LogDebug("InstallerApplicationNewStartModel -> OnGet");

        // User is at start of journey - reset session variables.
        var pageHistoryModel = new PageHistoryModel { CurrentPagePath = HttpContext.Request.Path };
        HttpContext.Session.Put(PageHistoryAttribute.PageHistorySessionKey, pageHistoryModel);
        ClearPageModel();
        _session.SearchBy = string.Empty;
        _session.SelectedFilterValue = string.Empty;
        _session.SelectedFilterKey = string.Empty;

        return Page();
    }
}
