using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Utilities;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;

[PageHistory]
public class PropertyOwnerContactDetailsModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether the property owner can give their consent online")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    /// <summary>
    /// PropertyOwnerEmailAddress.
    /// </summary>
    [BindProperty]
    public string? PropertyOwnerEmailAddress { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<PropertyOwnerContactDetailsModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public PropertyOwnerContactDetailsModel(ILogger<PropertyOwnerContactDetailsModel> logger)
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

        if (sessionModel.CanConsentOnline != null)
        {
            QuestionResponse = sessionModel.CanConsentOnline;
        }

        if (sessionModel.PropertyOwnerEmailAddress != null)
        {
            PropertyOwnerEmailAddress = sessionModel.PropertyOwnerEmailAddress;
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

        if (QuestionResponse == null)
        {
            return OnGet();
        }

        if (QuestionResponse == true)
        {
            if (PropertyOwnerEmailAddress == null)
            {
                ModelState.AddModelError(nameof(PropertyOwnerEmailAddress), "Enter the property owner's email address");
                return OnGet();
            }

            if (PropertyOwnerEmailAddress.Length > 254)
            {
                ModelState.AddModelError(nameof(PropertyOwnerEmailAddress), "Property owner's email address must be 254 characters or less");
                return OnGet();
            }

            if (!RegexUtilities.IsValidEmail(PropertyOwnerEmailAddress))
            {
                ModelState.AddModelError(nameof(PropertyOwnerEmailAddress), "Enter a valid email");
                return OnGet();
            }
        }

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        sessionModel.PropertyOwnerEmailAddress = PropertyOwnerEmailAddress;
        sessionModel.CanConsentOnline = QuestionResponse;

        if (QuestionResponse == false)
        {
            sessionModel.PropertyOwnerEmailAddress = string.Empty;
        }

        HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        return RedirectToPage(@Routes.Pages.Path.CD169, null, "");
    }
}
