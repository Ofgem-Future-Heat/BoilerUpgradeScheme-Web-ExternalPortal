using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;

[PageHistory]
public class PropertyOwnerAddressFormModel : AbstractFormPage
{
    private readonly ILogger<PropertyOwnerAddressFormModel> _logger;

    [BindProperty]
    [Required(ErrorMessage = "Enter the first line of the address")]
    [MaxLength(100, ErrorMessage = "Address line 1 must be 100 characters or less")]
    public string AddressLine1 { get; set; } = string.Empty;

    [BindProperty]
    [MaxLength(100, ErrorMessage = "Address line 2 must be 100 characters or less")]
    public string? AddressLine2 { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter a town or city")]
    [MaxLength(100, ErrorMessage = "Town or city must be 100 characters or less")]
    public string Town { get; set; } = string.Empty;

    [BindProperty]
    [MaxLength(100, ErrorMessage = "County must be 100 characters or less")]
    public string? County { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter a postal code or zip code")]
    [MaxLength(10, ErrorMessage = "Postal code or zip code must be 10 characters or less")]
    
    public string Postcode { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Enter a country")]
    [MaxLength(60, ErrorMessage = "Country must be 60 characters or less")]
    public string Country { get; set; } = string.Empty;

    public PropertyOwnerAddressFormModel(ILogger<PropertyOwnerAddressFormModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

    public IActionResult OnGet()
    {
        _logger.LogInformation("PropertyOwnerPostcodeModel -> OnGet");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (sessionModel.PropertyOwnerAddressPostcode != null)
        {
            Postcode = sessionModel.PropertyOwnerAddressPostcode;
        }

        if (sessionModel.PropertyOwnerAddress != null)
        {
            AddressLine1 = sessionModel.PropertyOwnerAddress.AddressLine1;
            AddressLine2 = sessionModel.PropertyOwnerAddress.AddressLine2;
            Town = sessionModel.PropertyOwnerAddress.AddressLine3 ?? string.Empty;
            County = sessionModel.PropertyOwnerAddress.County;
            Postcode = sessionModel.PropertyOwnerAddressPostcode;
            Country = sessionModel.PropertyOwnerAddress.Country;
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        _logger.LogInformation("PropertyOwnerPostcodeModel -> OnGet");

       
        if (!ModelState.IsValid)
        {
            List<string> fieldOrder = new List<string>(new string[] {
                            "AddressLine1", "Town", "Postcode", "County", "Country" })
                            .Select(f => f.ToLower()).ToList();
            //custom code for ordering
            if (ModelState != null)
            {
                ViewData["SortedErrors"] = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(m => new { Order = fieldOrder.IndexOf(m.Key.ToLower()), Error = m.Value })
                    .OrderBy(m => m.Order)
                    .SelectMany(m => m.Error.Errors.Select(e => e.ErrorMessage))
                    .ToArray();

                ViewData["SortedErrorsKey"] = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(m => new { Order = fieldOrder.IndexOf(m.Key.ToLower()), Key = m.Key })
                    .OrderBy(m => m.Order)
                    .Select(l => l.Key).ToArray();
            }
            return OnGet();
        }

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        sessionModel.PropertyOwnerAddress = new Address
        {
            AddressLine1 = AddressLine1,
            AddressLine2 = AddressLine2,
            AddressLine3 = Town,
            County = County,
            Postcode = Postcode,
            Country = Country
        };
        HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        return RedirectToPage(@Routes.Pages.Path.CD022, null, "");
    }
}
