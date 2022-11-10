using Microsoft.AspNetCore.Mvc;
using Ofgem.Lib.BUS.OSPlaces.Client.Domain.Exceptions;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Utilities;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;

[PageHistory]
public class PropertyOwnerPostcodeModel : AbstractFormPage
{
    private readonly ILogger<PropertyOwnerPostcodeModel> _logger;
    private readonly IPostcodeLookupService _postcodeLookupService;

    public const string PropertyOwnerAddressesSessionKey = "PropertyOwnerAddresses";

    public PropertyOwnerPostcodeModel(ILogger<PropertyOwnerPostcodeModel> logger, IPostcodeLookupService postcodeLookupService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _postcodeLookupService = postcodeLookupService ?? throw new ArgumentNullException(nameof(postcodeLookupService));
    }

    [Required(ErrorMessage = "Enter a postcode")]
    [StringLength(8, MinimumLength = 5, ErrorMessage = "Enter a postcode in the correct format")]
    [BindProperty]
    public string PropertyOwnerAddressPostcode { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        _logger.LogInformation("PropertyOwnerPostcodeModel -> OnGet");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (sessionModel?.PropertyOwnerAddressPostcode != null)
        {
            PropertyOwnerAddressPostcode = sessionModel.PropertyOwnerAddressPostcode;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("PropertyOwnerPostcodeModel -> OnPostAsync");

        var parsedPostcode = RegexUtilities.ParsePostcode(PropertyOwnerAddressPostcode);

        if (string.IsNullOrWhiteSpace(parsedPostcode) && !ModelState.ContainsKey(nameof(PropertyOwnerAddressPostcode)))
        {
            ModelState.AddModelError(nameof(PropertyOwnerAddressPostcode), "Enter a postcode in the correct format");
        }


        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Validation failed - returning to current page");

            return OnGet();
        }

        try
        {
            HttpContext.Session.Remove(PropertyOwnerAddressesSessionKey);

            var postcodeLookupResult = await _postcodeLookupService.GetAddresses(parsedPostcode);

            var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
            if (sessionModel == null)
            {
                return RedirectToPage(@Routes.Pages.Path.CD151);
            }

            sessionModel.PropertyOwnerAddressPostcode = parsedPostcode;
            HttpContext.Session.Put(PageModelSessionKey, sessionModel);

            if (postcodeLookupResult.TotalResults == 0)
            {
                ModelState.AddModelError(nameof(PropertyOwnerAddressPostcode), "No addresses found for this postcode");
                return OnGet();
            }

            HttpContext.Session.Put(PropertyOwnerAddressesSessionKey, postcodeLookupResult.Addresses);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is PostcodeFormatException)
            {
                _logger.LogWarning(ex, "Format error for postcode {PropertyOwnerAddressPostcode}", PropertyOwnerAddressPostcode);
                ModelState.AddModelError(nameof(PropertyOwnerAddressPostcode), "Enter a postcode in the correct format");
                return Page();
            }

            _logger.LogError(ex, "Error calling postcode lookup for postcode {PropertyOwnerAddressPostcode}", PropertyOwnerAddressPostcode);
            ModelState.AddModelError(nameof(PropertyOwnerAddressPostcode), "There was a problem retrieving addresses for this postcode. Please try again.");
            return Page();
        }

       
        return RedirectToPage(@Routes.Pages.Path.CD173, null, "");
    }
}
