using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.TechType;

[PageHistory]
public class TechSharedModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether you are installing the heat pump on a shared ground or water loop")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<TechSharedModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public TechSharedModel(ILogger<TechSharedModel> logger)
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

        if (sessionModel.IsSharedGroundLoop != null)
        {
            QuestionResponse = sessionModel.IsSharedGroundLoop;
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

        if (!ModelState.IsValid)
        {
            return OnGet();
        }

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        sessionModel.IsSharedGroundLoop = QuestionResponse;

        if (QuestionResponse != null && QuestionResponse == true)
        {
            sessionModel.TechTypeId = TechTypes.SharedGroundLoopSourceHeatPump;
        }

        HttpContext.Session.Put(PageModelSessionKey, sessionModel);
        return RedirectToPage(@Routes.Pages.Path.CD012, null, "");
    }
}
