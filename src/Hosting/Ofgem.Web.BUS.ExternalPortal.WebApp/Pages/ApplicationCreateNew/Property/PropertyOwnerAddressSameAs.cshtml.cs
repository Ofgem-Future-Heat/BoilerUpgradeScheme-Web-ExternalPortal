using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;

[PageHistory]
public class PropertyOwnerAddressSameAsModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether the property owner lives at the installation address")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    public Address? InstallationAddress { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<PropertyOwnerAddressSameAsModel> _logger;

    public CreateApplicationModel CreateApplication { get; set; } = null!;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public PropertyOwnerAddressSameAsModel(ILogger<PropertyOwnerAddressSameAsModel> logger)
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

        CreateApplication = sessionModel;

        if (sessionModel.PropertyOwnerAddressIsInstallationAddress != null)
        {
            QuestionResponse = sessionModel.PropertyOwnerAddressIsInstallationAddress;
        }

        if (sessionModel.InstallationAddress != null)
        {
            InstallationAddress = sessionModel.InstallationAddress;
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

        sessionModel.PropertyOwnerAddressIsInstallationAddress = QuestionResponse;

        if (QuestionResponse == true)
        {
            sessionModel.PropertyOwnerAddress = sessionModel.InstallationAddress;
            sessionModel.PropertyOwnerAddressPostcode = sessionModel.InstallationAddressPostcode;
        }

        HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        if (QuestionResponse == true)
        {
            return RedirectToPage(@Routes.Pages.Path.CD022, null, "");
        }

        return RedirectToPage(@Routes.Pages.Path.CD172, null, "");
    }
}
