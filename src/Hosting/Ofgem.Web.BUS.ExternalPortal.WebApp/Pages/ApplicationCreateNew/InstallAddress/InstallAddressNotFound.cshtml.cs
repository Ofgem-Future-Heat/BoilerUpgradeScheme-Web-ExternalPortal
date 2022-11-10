using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.InstallAddress;

public class InstallAddressNotFoundModel : AbstractFormPage
{
    private readonly ILogger<InstallAddressNotFoundModel> _logger;

    /// <summary>
    /// The installation address postcode entered on the previous step.
    /// </summary>
    public string Postcode { get; set; } = string.Empty;

    public InstallAddressNotFoundModel(ILogger<InstallAddressNotFoundModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IActionResult OnGet()
    {
        _logger.LogInformation("InstallAddressNotFoundModel -> OnGet");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (string.IsNullOrEmpty(sessionModel.InstallationAddressPostcode))
        {
            return RedirectToPage(@Routes.Pages.Path.CD005, null, "");
        }

        Postcode = sessionModel.InstallationAddressPostcode;

        return Page();
    }

    public IActionResult OnPost(string redirectUrl)
    {
        _logger.LogInformation("InstallAddressNotFoundModel -> OnPost");

        HttpContext.Session.Remove(PageModelSessionKey);
        HttpContext.Session.Remove(PageHistoryAttribute.PageHistorySessionKey);

        return Redirect(redirectUrl);
    }
}
