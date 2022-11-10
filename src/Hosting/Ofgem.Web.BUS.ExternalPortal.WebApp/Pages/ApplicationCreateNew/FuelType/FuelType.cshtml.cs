using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.FuelType;

[PageHistory]
public class FuelTypeModel : AbstractFormPage
{
    [BindProperty]
    [Required(ErrorMessage = "Choose the primary fuel type that the new heating system is replacing")]
    public string FuelType { get; set; } = string.Empty;

    [BindProperty]
    public string? FuelTypeOther { get; set; }


    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<FuelTypeModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public FuelTypeModel(ILogger<FuelTypeModel> logger)
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

        if (sessionModel.PreviousFuelType != null)
        {
            FuelType = sessionModel.PreviousFuelType;
            FuelTypeOther = sessionModel.PreviousFuelTypeOther;
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

        if (string.CompareOrdinal(FuelType.ToLower(), "other") == 0 && string.IsNullOrWhiteSpace(FuelTypeOther))
        {
            ModelState.AddModelError(nameof(FuelTypeOther), "Enter the other type of fuel that the new heating system is replacing");
        }

        if (string.CompareOrdinal(FuelType.ToLower(), "other") == 0 && !string.IsNullOrWhiteSpace(FuelTypeOther) && FuelTypeOther.Length > 100)
        {
            ModelState.AddModelError(nameof(FuelTypeOther), "Other type of fuel must be 100 characters or fewer");
        }

        if (!string.IsNullOrWhiteSpace(FuelTypeOther) && string.IsNullOrWhiteSpace(FuelType))
        {
            ModelState.AddModelError(nameof(FuelType), "Enter the other type of fuel that the new heating system is replacing");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (FuelType.ToLower() != "other")
        {
            FuelTypeOther = string.Empty;
        }

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        sessionModel.PreviousFuelType = FuelType;
        sessionModel.PreviousFuelTypeOther = FuelTypeOther;

        HttpContext.Session.Put(PageModelSessionKey, sessionModel);
        return RedirectToPage(@Routes.Pages.Path.CD019, null, "");
    }
}
