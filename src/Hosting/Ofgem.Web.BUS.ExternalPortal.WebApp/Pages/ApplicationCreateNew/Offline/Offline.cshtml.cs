using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Offline;

public class OfflineModel : AbstractFormPage
{
    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<OfflineModel> _logger;

    public string ErrorMessage { get; set; } = null!;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public OfflineModel(ILogger<OfflineModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnGet(string contentSelection)
    {
        return Page();
    }
}
