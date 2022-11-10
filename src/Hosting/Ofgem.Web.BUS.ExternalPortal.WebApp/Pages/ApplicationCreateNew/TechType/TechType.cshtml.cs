using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.TechType;

[PageHistory]
public class TechTypeModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Choose a heating type")]
    [BindProperty]
    public Guid QuestionResponse { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<TechTypeModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public TechTypeModel(ILogger<TechTypeModel> logger)
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

        if (sessionModel?.TechTypeId != null)
        {
            if (sessionModel.TechTypeId == TechTypes.SharedGroundLoopSourceHeatPump)
            {
                QuestionResponse = TechTypes.GroundSourceHeatPump;
            }
            else
            {
                QuestionResponse = sessionModel.TechTypeId.Value;
            }
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

        CreateApplicationModel sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey) ?? new CreateApplicationModel();

        /* Options have been changed - clear the session */
        if (sessionModel.TechTypeId != null && QuestionResponse != sessionModel.TechTypeId)
        {
            sessionModel.TechSpecProjectEligible = null;
            sessionModel.IsSharedGroundLoop = null;
            sessionModel.BiomassEligible = null;
        }

        if (QuestionResponse == TechTypes.AirSourceHeatPump
            || QuestionResponse == TechTypes.GroundSourceHeatPump
            || QuestionResponse == TechTypes.BiomassBoiler)
        {
            sessionModel.TechTypeId = QuestionResponse;
            HttpContext.Session.Put(PageModelSessionKey, sessionModel);

            if (QuestionResponse == TechTypes.AirSourceHeatPump || QuestionResponse == TechTypes.BiomassBoiler)
            {
                return RedirectToPage(@Routes.Pages.Name.CD012, null, "");
            }
            else
            {
                return RedirectToPage(@Routes.Pages.Name.CD179, null, "");
            }
        }
        else
        {
            return Page();
        }
    }
}
