using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationHelp;

public class InstallerHelpModel : PageModel
{
    private readonly ILogger<InstallerHelpModel> _logger;
    private readonly IExternalBusinessAccountService _businessAccountsService;

    public InstallerHelpModel(SessionService session, IExternalBusinessAccountService businessAccountsService, ILogger<InstallerHelpModel> logger)
    {
        _businessAccountsService = businessAccountsService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        BusinessAccountId = session.BusinessAccountId;
    }

    public string BusinessAccountNumber { get; set; } = null!;

    public Guid? BusinessAccountId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("InstallerHelpModel -> OnGet");

        if (BusinessAccountId == null || BusinessAccountId == Guid.Empty)
        {
            return RedirectToPage("/Shared/InternalError");
        }

        var businessAccountData = await _businessAccountsService.ExternalGetBusinessAccountAsync(BusinessAccountId);

        BusinessAccountNumber = businessAccountData.BusinessAccountNumber ?? string.Empty;

        return Page();
    }
}
