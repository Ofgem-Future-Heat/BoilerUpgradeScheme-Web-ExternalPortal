using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;

[PageHistory]
public class PropertyOwnerNameModel : AbstractFormPage
{
    /// <summary>
    /// PropertyOwnerName.
    /// </summary>
    [MaxLength(100, ErrorMessage = "Property owner's name must be 100 characters or less")]
    [Required(ErrorMessage = "Enter the property owner's full name")]
    [BindProperty]
    public string? PropertyOwnerName { get; set; }

    /// <summary>
    /// PropertyOwnerTelephoneNumber.
    /// </summary>
    [Required(ErrorMessage = "Enter the property owner's telephone number")]
    [MaxLength(100, ErrorMessage = "Property owner's telephone number must be 100 characters or less")]
    [BindProperty]
    public string? PropertyOwnerTelephoneNumber { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<PropertyOwnerNameModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public PropertyOwnerNameModel(ILogger<PropertyOwnerNameModel> logger)
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

        if (sessionModel.PropertyOwnerName != null)
        {
            PropertyOwnerName = sessionModel.PropertyOwnerName;
        }

        if (sessionModel.PropertyOwnerTelephoneNumber != null)
        {
            PropertyOwnerTelephoneNumber = sessionModel.PropertyOwnerTelephoneNumber;
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

        sessionModel.PropertyOwnerName = PropertyOwnerName;
        sessionModel.PropertyOwnerTelephoneNumber = PropertyOwnerTelephoneNumber;

        HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        return RedirectToPage(@Routes.Pages.Path.CD168, null, "");
    }
}
