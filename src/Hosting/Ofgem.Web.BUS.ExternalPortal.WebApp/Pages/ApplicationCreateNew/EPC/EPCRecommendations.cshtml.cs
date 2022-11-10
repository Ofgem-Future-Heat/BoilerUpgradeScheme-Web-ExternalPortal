using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.EPC;

[PageHistory]
public class EPCRecommendationsModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether the property's EPC report has recommendations for loft or cavity wall insulation")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<EPCRecommendationsModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public EPCRecommendationsModel(ILogger<EPCRecommendationsModel> logger)
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

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (sessionModel.EpcHasRecommendations != null)
        {
            QuestionResponse = sessionModel.EpcHasRecommendations;
        }

        return Page();
    }

    /// <summary>
    /// OnPostAsync - Gets the user decision for the next action.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnPost()
    {
        _logger.LogInformation("public IActionResult OnPost()");

        if (!ModelState.IsValid || QuestionResponse == null)
        {
            return Page();
        }

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        sessionModel.EpcHasRecommendations = QuestionResponse;
        
        if (QuestionResponse == false)
        {
            sessionModel.EpcHasExemptions = null;
        }

        HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        if (QuestionResponse == true)
        {
            return RedirectToPage(@Routes.Pages.Path.CD165, null, "");
        }

        return RedirectToPage(@Routes.Pages.Path.CD167, null, "");
    }
}
