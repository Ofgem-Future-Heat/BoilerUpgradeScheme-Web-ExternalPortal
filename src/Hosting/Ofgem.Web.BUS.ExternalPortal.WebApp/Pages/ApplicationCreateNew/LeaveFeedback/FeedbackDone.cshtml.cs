using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.LeaveFeedback;

public class FeedbackDoneModel : AbstractFormPage
{
    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<FeedbackDoneModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public FeedbackDoneModel(ILogger<FeedbackDoneModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// OnPost - gets the user response go back home!
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnPostHomeButton()
    {
        _logger.LogInformation("public IActionResult OnPost()");

        return RedirectToPage("/ApplicationsDashboard/InstallerApplications");
    }
}
