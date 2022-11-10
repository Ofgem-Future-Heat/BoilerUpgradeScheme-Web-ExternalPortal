using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Confirmation;

public class ConfirmationModel : AbstractFormPage
{
    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<ConfirmationModel> _logger;

    public ApplicationConfirmationModel ApplicationConfirmationModel { get; set; } = new();

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public ConfirmationModel(ILogger<ConfirmationModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnGet()
    {
        _logger.LogInformation("public IActionResult OnGet()");

        ApplicationConfirmationModel = TempData.Get<ApplicationConfirmationModel>("ApplicationConfirmationDataModel") ??
            throw new Exception("Could not load application confirmation from temp data");

        return Page();
    }
}
