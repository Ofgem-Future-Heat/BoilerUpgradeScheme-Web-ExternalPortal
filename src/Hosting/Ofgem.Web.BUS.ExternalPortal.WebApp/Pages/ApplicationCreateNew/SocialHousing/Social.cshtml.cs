using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.SocialHousing;

[PageHistory]
public class SocialModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether the property meets the definition of 'social housing'")]
    [BindProperty]
    public bool? QuestionResponse { get; set; } = null!;

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<SocialModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public SocialModel(ILogger<SocialModel> logger)
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

        if (sessionModel.IsSocialHousing != null)
        {
            QuestionResponse = sessionModel.IsSocialHousing;
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
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (QuestionResponse == true)
        {
            sessionModel.IsSocialHousing = true;
            HttpContext.Session.Put(PageModelSessionKey, sessionModel);
            return RedirectToPage(@Routes.Pages.Path.CD004, new { ContentSelection = "CD161" });
        }

        sessionModel.IsSocialHousing = false;
        HttpContext.Session.Put(PageModelSessionKey, sessionModel);
        return RedirectToPage(@Routes.Pages.Path.CD009, null, "");
    }
}
