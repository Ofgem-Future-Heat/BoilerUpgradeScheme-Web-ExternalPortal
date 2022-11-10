using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.NewBuild;

[PageHistory]
public class NewBuildModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether the property is a new build")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    public bool IsBiomass { get; set; } = false;

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<NewBuildModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public NewBuildModel(ILogger<NewBuildModel> logger)
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

        if (sessionModel.IsNewBuild != null)
        {
            QuestionResponse = sessionModel.IsNewBuild;
        }
        if (sessionModel.TechTypeId != null)
        {
            IsBiomass = sessionModel.TechTypeId == TechTypes.BiomassBoiler;
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
            return OnGet();
        }

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel?.TechTypeId == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        sessionModel.IsNewBuild = QuestionResponse;

        if (QuestionResponse == true)
        {
            if (sessionModel.TechTypeId == TechTypes.BiomassBoiler)
            {
                HttpContext.Session.Put(PageModelSessionKey, sessionModel);
                return RedirectToPage(@Routes.Pages.Path.CD004, new { ContentSelection = "CD09" });
            }
            else if (sessionModel.TechTypeId == TechTypes.AirSourceHeatPump
                    || sessionModel.TechTypeId == TechTypes.GroundSourceHeatPump
                    || sessionModel.TechTypeId == TechTypes.SharedGroundLoopSourceHeatPump)
            {
                sessionModel.HasEpc = null;
                sessionModel.EpcReferenceNumber = null;
                sessionModel.EpcHasRecommendations = null;
                sessionModel.EpcHasExemptions = null;

                HttpContext.Session.Put(PageModelSessionKey, sessionModel);
                return RedirectToPage(@Routes.Pages.Path.CD010, null, "");
            }
        }
        else if (QuestionResponse == false)
        {
            sessionModel.IsEligibleSelfBuild = null;
        }

        HttpContext.Session.Put(PageModelSessionKey, sessionModel);
        return RedirectToPage(@Routes.Pages.Path.CD162, null, "");
    }
}

