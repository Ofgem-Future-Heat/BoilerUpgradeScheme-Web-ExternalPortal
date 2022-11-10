using Microsoft.AspNetCore.Mvc;
using Ofgem.Lib.BUS.OSPlaces.Client.Domain.Exceptions;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Utilities;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.InstallAddress;

[PageHistory]
public class InstallPostcodeModel : AbstractFormPage
{
    private readonly ILogger<InstallPostcodeModel> _logger;
    private readonly IPostcodeLookupService _postcodeLookupService;

    public const string InstallationAddressesSessionKey = "InstallationAddresses";

    public InstallPostcodeModel(ILogger<InstallPostcodeModel> logger, IPostcodeLookupService postcodeLookupService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _postcodeLookupService = postcodeLookupService ?? throw new ArgumentNullException(nameof(postcodeLookupService));
    }

    [Required(ErrorMessage = "Enter a postcode")]
    [StringLength(8, MinimumLength = 5, ErrorMessage = "Enter a postcode in the correct format")]
    [BindProperty]
    public string InstallationAddressPostcode { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        _logger.LogInformation("InstallPostcodeModel -> OnGet");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (sessionModel?.InstallationAddressPostcode != null)
        {
            InstallationAddressPostcode = sessionModel.InstallationAddressPostcode;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("InstallPostcodeModel -> OnPostAsync");

        var parsedPostcode = RegexUtilities.ParsePostcode(InstallationAddressPostcode);

        if (string.IsNullOrWhiteSpace(parsedPostcode) && !ModelState.ContainsKey(nameof(InstallationAddressPostcode)))
        {
            ModelState.AddModelError(nameof(InstallationAddressPostcode), "Enter a postcode in the correct format");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Validation failed - returning to current page");

            return OnGet();
        }

        try
        {
            HttpContext.Session.Remove(InstallationAddressesSessionKey);

            var postcodeLookupResult = await _postcodeLookupService.GetEnglishWelshAddresses(parsedPostcode);

            if (postcodeLookupResult.TotalResults > 0 && postcodeLookupResult.FilteredResults == 0)
            {
                return RedirectToPage(@Routes.Pages.Path.CD004, new { ContentSelection = "CD05" });
            }

            var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
            if (sessionModel == null)
            {
                return RedirectToPage(@Routes.Pages.Path.CD151);
            }

            sessionModel.InstallationAddressPostcode = parsedPostcode;
            HttpContext.Session.Put(PageModelSessionKey, sessionModel);

            if (postcodeLookupResult.TotalResults == 0)
            {
                ModelState.AddModelError(nameof(InstallationAddressPostcode), "No addresses found for this postcode");
                return OnGet();

            }

            HttpContext.Session.Put(InstallationAddressesSessionKey, postcodeLookupResult.Addresses);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is PostcodeFormatException)
            {
                _logger.LogWarning(ex, "Format error for postcode {InstallationAddressPostcode}", InstallationAddressPostcode);
                ModelState.AddModelError(nameof(InstallationAddressPostcode), "Enter a postcode in the correct format");
                return Page();
            }

            _logger.LogError(ex, "Error calling postcode lookup for postcode {InstallationAddressPostcode}", InstallationAddressPostcode);
            ModelState.AddModelError(nameof(InstallationAddressPostcode), "There was a problem retrieving addresses for this postcode. Please try again.");
            return Page();
        }

        return RedirectToPage(@Routes.Pages.Path.CD029, null, "");
    }
}
