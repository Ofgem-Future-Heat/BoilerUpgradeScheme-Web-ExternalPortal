using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem.Lib.BUS.OSPlaces.Client.Domain.DTOs.Responses;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.InstallAddress;

[PageHistory]
public class InstallAddressModel : AbstractFormPage
{
    private readonly ILogger<InstallAddressModel> _logger;
    private readonly IExternalApplicationsService _applicationsService;

    public CreateApplicationModel CreateApplication { get; set; } = null!;

    /// <summary>
    /// List of UPRNs (key) and addresses (value) to be bound to the addresses dropdown.
    /// </summary>
    public SelectList Addresses { get; set; } = null!;

    /// <summary>
    /// The installation address postcode entered on the previous step.
    /// </summary>
    public string Postcode { get; set; } = string.Empty;

    /// <summary>
    /// Selected dropdown key.
    /// </summary>
    [BindProperty]
    [Required(ErrorMessage = "Choose an address")]
    public string SelectedAddressUprn { get; set; } = string.Empty;

    public InstallAddressModel(ILogger<InstallAddressModel> logger, IExternalApplicationsService applicationsService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));

    }

    public IActionResult OnGet()
    {
        _logger.LogInformation("InstallAddressModel -> OnGet");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        var sessionAddresses = HttpContext.Session.GetOrDefault<IEnumerable<AddressResult>>(InstallPostcodeModel.InstallationAddressesSessionKey);

        if (sessionAddresses == null || !sessionAddresses.Any() || string.IsNullOrEmpty(sessionModel.InstallationAddressPostcode))
        {
            return RedirectToPage(@Routes.Pages.Path.CD005, null, "");
        }

        Addresses = new SelectList(sessionAddresses, "Uprn", "Address");
        Postcode = sessionModel.InstallationAddressPostcode;

        if (sessionModel?.InstallationAddress?.UPRN != null)
        {
            SelectedAddressUprn = sessionModel.InstallationAddress.UPRN;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()

    {
        _logger.LogInformation("InstallAddressModel -> OnPost");

        if (!ModelState.IsValid)
        {
            return OnGet();
        }

        var sessionAddresses = HttpContext.Session.GetOrDefault<IEnumerable<AddressResult>>(InstallPostcodeModel.InstallationAddressesSessionKey)
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

        try
        {
            //check if duplicate application
            var checkDuplicateApplication = await _applicationsService.CheckDuplicateApplicationAsync(selectedAddress.Uprn);
            if (checkDuplicateApplication == true)
            {
                
                TempData.Put("installationAddress", selectedAddress.Address.ToString().Replace(",","</br>"));
                
                return RedirectToPage("/ApplicationCreateNew/DropOut/DropOut", new { ContentSelection = "CD29" });
            }

        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking application");
            ModelState.AddModelError("", ex.Message);
        }


        
        sessionModel.InstallationAddress = new Address(selectedAddress);
        sessionModel.InstallationAddressManualEntry = false;
        HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        return RedirectToPage(@Routes.Pages.Path.CD158, null, "");
    }
}
