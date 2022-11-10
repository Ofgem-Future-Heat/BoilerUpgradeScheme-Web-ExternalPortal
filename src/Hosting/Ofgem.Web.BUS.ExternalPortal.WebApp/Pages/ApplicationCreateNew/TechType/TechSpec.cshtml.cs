using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.TechType;

[PageHistory]
public class TechSpecModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether your project will meet these eligibility critieria")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    public Guid TechTypeId { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<TechSpecModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public TechSpecModel(ILogger<TechSpecModel> logger)
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
        if (sessionModel?.TechTypeId == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (sessionModel.TechSpecProjectEligible != null)
        {
            QuestionResponse = sessionModel.TechSpecProjectEligible;
        }

        TechTypeId = sessionModel.TechTypeId.Value;

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

        if (QuestionResponse != null && QuestionResponse == true)
        {
            sessionModel.TechSpecProjectEligible = true;
            HttpContext.Session.Put(PageModelSessionKey, sessionModel);
            if (sessionModel.TechTypeId == TechTypes.BiomassBoiler)
            {
                return RedirectToPage(@Routes.Pages.Path.CD157, null, "");
            }
            else
            {
                return RedirectToPage(@Routes.Pages.Path.CD005, null, "");
            }
        }

        sessionModel.TechSpecProjectEligible = false;
        HttpContext.Session.Put(PageModelSessionKey, sessionModel);
        return RedirectToPage(@Routes.Pages.Path.CD004, new { ContentSelection = "CD12" });
    }
}
