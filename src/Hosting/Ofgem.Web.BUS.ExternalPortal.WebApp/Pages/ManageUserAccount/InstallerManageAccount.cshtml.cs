using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationsDashboard;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ManageUserAccount;

public class InstallerManageAccountModel : PageModel
{
    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<InstallerApplicationsModel> _logger;

    /// <summary>
    /// Session handler for user interactions.
    /// </summary>
    private readonly SessionService _session;

    public InstallerManageAccountModel(ILogger<InstallerApplicationsModel> logger,
                                      SessionService session)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _session.SearchBy = string.Empty;
        _session.SelectedFilterValue = string.Empty;
        _session.SelectedFilterKey = string.Empty;


    }
    public void OnGet()
    {
    }
}
