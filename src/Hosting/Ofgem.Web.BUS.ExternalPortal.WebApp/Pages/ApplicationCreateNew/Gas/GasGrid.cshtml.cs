using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Gas;

[PageHistory]
public class GasGridModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether the property is on the gas grid")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<GasGridModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public GasGridModel(ILogger<GasGridModel> logger)
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

        if (sessionModel.IsOnGasGrid != null)
        {
            QuestionResponse = sessionModel.IsOnGasGrid;
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
        if (sessionModel?.TechTypeId == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (QuestionResponse != null)
        {
            sessionModel.IsOnGasGrid = QuestionResponse;
            HttpContext.Session.Put(PageModelSessionKey, sessionModel);

            if ((QuestionResponse == true && sessionModel.TechTypeId != TechTypes.BiomassBoiler)
                || (QuestionResponse == false))
            {
                return new RedirectToPageResult(@Routes.Pages.Path.CD161);
            }
        }

        return new RedirectToPageResult(@Routes.Pages.Path.CD004, new { ContentSelection = "CD158" });

    }
}
