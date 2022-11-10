using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCancel;

public class InstallerApplicationVoucherCancelModel : PageModel
{
    private readonly ILogger<InstallerApplicationVoucherCancelModel> _logger;
    private readonly IExternalBusinessAccountService _businessAccountsService;

    public InstallerApplicationVoucherCancelModel(SessionService session, IExternalBusinessAccountService businessAccountsService, ILogger<InstallerApplicationVoucherCancelModel> logger)
    {
        _businessAccountsService = businessAccountsService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        BusinessAccountId = session.BusinessAccountId;
        ApplicationId = session.ReferenceNumber;
    }

    public string BusinessAccountNumber { get; set; } = null!;

    public Guid? BusinessAccountId { get; set; }

    public string ApplicationId { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("InstallerApplicationVoucherCancelModel -> OnGet");

        if (BusinessAccountId == null || string.IsNullOrWhiteSpace(ApplicationId))
        {
            return RedirectToPage("/Shared/InternalError");
        }

        var businessAccountData = await _businessAccountsService.ExternalGetBusinessAccountAsync(BusinessAccountId);

        BusinessAccountNumber = businessAccountData.BusinessAccountNumber ?? string.Empty;

        return Page();
    }
}
