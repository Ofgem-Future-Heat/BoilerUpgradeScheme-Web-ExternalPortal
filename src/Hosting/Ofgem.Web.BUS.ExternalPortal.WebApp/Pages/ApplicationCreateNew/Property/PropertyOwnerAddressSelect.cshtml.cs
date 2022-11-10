using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem.Lib.BUS.OSPlaces.Client.Domain.DTOs.Responses;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;

[PageHistory]
public class PropertyOwnerAddressSelectModel : AbstractFormPage
{
    private readonly ILogger<PropertyOwnerAddressSelectModel> _logger;

    /// <summary>
    /// List of UPRNs (key) and addresses (value) to be bound to the addresses dropdown.
    /// </summary>
    public SelectList Addresses { get; set; } = null!;

    /// <summary>
    /// The property owner address postcode entered on the previous step.
    /// </summary>
    public string Postcode { get; set; } = string.Empty;

    /// <summary>
    /// Selected dropdown key.
    /// </summary>
    [BindProperty]
    [Required(ErrorMessage = "Choose an address")]
    public string SelectedAddressUprn { get; set; } = string.Empty;

    public PropertyOwnerAddressSelectModel(ILogger<PropertyOwnerAddressSelectModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IActionResult OnGet()
    {
        _logger.LogInformation("PropertyOwnerAddressSelectModel -> OnGet");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        var sessionAddresses = HttpContext.Session.GetOrDefault<IEnumerable<AddressResult>>(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey);

        if (sessionAddresses == null || !sessionAddresses.Any() || string.IsNullOrEmpty(sessionModel.PropertyOwnerAddressPostcode))
        {
            return RedirectToPage(@Routes.Pages.Path.CD172, null, "");
        }

        Addresses = new SelectList(sessionAddresses, "Uprn", "Address");
        Postcode = sessionModel.PropertyOwnerAddressPostcode;

        if (sessionModel?.PropertyOwnerAddress?.UPRN != null)
        {
            SelectedAddressUprn = sessionModel.PropertyOwnerAddress.UPRN;
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        _logger.LogInformation("PropertyOwnerAddressSelectModel -> OnPost");

        if (!ModelState.IsValid)
        {
            return OnGet();
        }

        var sessionAddresses = HttpContext.Session.GetOrDefault<IEnumerable<AddressResult>>(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey)
                               ?? Enumerable.Empty<AddressResult>();
        var selectedAddress = sessionAddresses.FirstOrDefault(a => a.Uprn.Equals(SelectedAddressUprn, StringComparison.OrdinalIgnoreCase));

        if (selectedAddress == null)
        {
            ModelState.AddModelError(nameof(SelectedAddressUprn), "Choose an address");
            return OnGet();
        }

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        sessionModel.PropertyOwnerAddress = new Address(selectedAddress);
        HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        return RedirectToPage(@Routes.Pages.Path.CD022, null, "");
    }
}
