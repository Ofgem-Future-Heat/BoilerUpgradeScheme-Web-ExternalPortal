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
public class InstallAddressManualModel : AbstractFormPage
{
    private readonly ILogger<InstallAddressManualModel> _logger;
    private readonly IPostcodeLookupService _postcodeLookupService;

    public const string InstallationAddressesSessionKey = "InstallationAddresses";

    public InstallAddressManualModel(ILogger<InstallAddressManualModel> logger, IPostcodeLookupService postcodeLookupService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _postcodeLookupService = postcodeLookupService ?? throw new ArgumentNullException(nameof(postcodeLookupService));
    }

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
    [RegularExpression(@"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})", ErrorMessage = "Enter a postcode in the correct format")]

    public string Postcode { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        _logger.LogInformation("InstallAddressManualModel -> OnGet");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (sessionModel.InstallationAddressPostcode != null)
        {
            Postcode = sessionModel.InstallationAddressPostcode;
        }

        if (sessionModel.InstallationAddress != null)
        {
            AddressLine1 = sessionModel.InstallationAddress.AddressLine1;
            AddressLine2 = sessionModel.InstallationAddress.AddressLine2;
            Town = sessionModel.InstallationAddress.AddressLine3;
            County = sessionModel.InstallationAddress.County;
            Postcode = sessionModel.InstallationAddressPostcode;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("InstallAddressManualModel -> OnGet");

        var parsedPostcode = RegexUtilities.ParsePostcode(Postcode);

        if (string.IsNullOrWhiteSpace(parsedPostcode) && !ModelState.ContainsKey(nameof(Postcode)))
        {
            ModelState.AddModelError(nameof(Postcode), "Enter a postcode in the correct format");
        }

        if (!ModelState.IsValid)
        {
            AddCustomErrorModel();
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

           
            HttpContext.Session.Put(InstallationAddressesSessionKey, postcodeLookupResult.Addresses);

            sessionModel.InstallationAddress = new Address
            {
                AddressLine1 = AddressLine1,
                AddressLine2 = AddressLine2,
                AddressLine3 = Town,
                County = County,
                Postcode = Postcode,
            };

            sessionModel.InstallationAddressManualEntry = true;

            HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        }
        catch (Exception ex)
        {
            if (ex.InnerException is PostcodeFormatException)
            {
                _logger.LogWarning(ex, "Format error for postcode {Postcode}", Postcode);
                ModelState.AddModelError(nameof(Postcode), "Enter a postcode in the correct format");
                AddCustomErrorModel();
                return Page();
            }

            _logger.LogError(ex, "Error calling postcode lookup for postcode {Postcode}", Postcode);
            ModelState.AddModelError(nameof(Postcode), "There was a problem retrieving addresses for this postcode. Please try again.");
            AddCustomErrorModel();
            _logger.LogInformation("Validation failed - returning to current page");
          
            return Page();
        }


       
        return RedirectToPage(@Routes.Pages.Path.CD158, null, "");
    }


    private void AddCustomErrorModel()
    {
        _logger.LogInformation("Validation failed - returning to current page");
        List<string> fieldOrder = new List<string>(new string[] {
                            "AddressLine1", "Town", "Postcode", "County", "Country" })
                       .Select(f => f.ToLower()).ToList();
        //custom code for ordering
        ViewData["SortedErrors"] = ModelState
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
}
