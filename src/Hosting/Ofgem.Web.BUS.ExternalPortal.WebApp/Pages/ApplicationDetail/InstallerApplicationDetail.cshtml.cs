using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.Applications.Domain.Constants;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using static Ofgem.API.BUS.Applications.Domain.ApplicationSubStatus;
using static Ofgem.API.BUS.Applications.Domain.VoucherSubStatus;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationDetail;

public class InstallerApplicationDetailModel : PageModel
{
    /// <summary>
    /// Interacts with the business client.
    /// </summary>
    private readonly IExternalBusinessAccountService _businessAccountsService;

    /// <summary>
    /// Interacts with the applications client.
    /// </summary>
    private readonly IExternalApplicationsService _applicationsService;

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<InstallerApplicationDetailModel> _logger;

    /// <summary>
    /// Data relating to the user who submitted the application
    /// </summary>
    public ExternalUserAccount? ApplicationSubmitter { get; set; }

    /// <summary>
    /// Data from the Business Account API to retrieve account data and validation.
    /// </summary>
    public BusinessAccount? BusinessAccount { get; set; }

    /// <summary>
    /// Data relating to the application currently being viewed.
    /// </summary>
    public Application? Application { get; set; }

    /// <summary>
    /// Data relating to the most recent consent request associated with an application.
    /// </summary>
    public ConsentRequest? ConsentRequest { get; set; }

    /// <summary>
    /// The current application or voucher status ID.
    /// </summary>
    public Guid CurrentStatusId { get; set; }

    public bool ShowConsentReceived { get; set; } = false;

    public bool ShowApplicationDetails { get; set; } = false;

    public bool ShowVoucherDetails { get; set; } = false;

    public bool ShowRedemptionDetails { get; set; } = false;

    /// <summary>
    /// Display the Voucher Redemption section.
    /// </summary>
    public bool IsVoucherRedeemed { get; set; } = false;

    /// <summary>
    /// Display the Cancel Application button.
    /// </summary>
    public bool ShowCancelApplicationButton { get; set; } = false;

    /// <summary>
    /// Display the Redeem voucher button.
    /// </summary>
    public bool ShowRedeemVoucherButton { get; set; } = false;

    /// <summary>
    /// Display the Cancel Voucher button.
    /// </summary>
    public bool ShowCancelVoucherButton { get; set; } = false;

    public string? CurrentContactFullName { get; set; }

    public string? CurrentContactEmailAddress { get; set; }

    /// <summary>
    /// Session handler for user interactions.
    /// </summary>
    private readonly SessionService _session;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <param name="businessAccountsService">Business Account Service end point.</param>
    /// <param name="applicationsService">Application Service end point.</param>
    /// <param name="session">Session for local user retained values.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public InstallerApplicationDetailModel(ILogger<InstallerApplicationDetailModel> logger,
                                           IExternalBusinessAccountService businessAccountsService,
                                           IExternalApplicationsService applicationsService,
                                           SessionService session)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _businessAccountsService = businessAccountsService ?? throw new ArgumentNullException(nameof(businessAccountsService));
        _applicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <param name="referenceNumber">The user selected reference number from the calling page.</param>
    public async Task<IActionResult> OnGetAsync(string referenceNumber)
    {
        _logger.LogTrace("InstallerApplicationDetailModel -> OnGetAsync");

        if (string.IsNullOrWhiteSpace(referenceNumber))
        {
            return RedirectToPage("/Shared/InternalError");
        }

        await GetApplicationData(referenceNumber);
        if (Application == null)
        {
            return RedirectToPage("/Shared/InternalError");
        }

        if (Application.BusinessAccountId != _session.BusinessAccountId)
        {
            return Redirect("/Shared/RoutingError?statusCode=403");
        }

        await GetBusinessAccountData(Application.BusinessAccountId);
        if (BusinessAccount == null)
        {
            return RedirectToPage("/Shared/InternalError");
        }

        await GetApplicationContacts(BusinessAccount.Id, Application.SubmitterId, Application.CurrentContactId);
        if (ApplicationSubmitter == null)
        {
            return RedirectToPage("/Shared/InternalError");
        }

        SetConsentReceivedState(CurrentStatusId);
        SetVoucherRedemptionState(CurrentStatusId);

        SetCancelVoucherButtonState(CurrentStatusId);
        SetRedeemVoucherButtonState(CurrentStatusId);
        SetCancelApplicationButtonState(CurrentStatusId);

        SetApplicationDetailsState(CurrentStatusId);
        SetVoucherDetailsState(CurrentStatusId);
        SetRedemptionDetailsState(CurrentStatusId);

        return Page();
    }

    /// <summary>
    /// Determine if to display the Consent Received section.
    /// IF application status is either:
    /// (A) In review AND consent received
    /// (A) With installer AND consent received
    /// (A) Consent review AND consent received
    /// (A) Consent revoked
    /// (A) QC
    /// (A) DA
    /// </summary>
    private void SetConsentReceivedState(Guid statusId)
    {
        var isConsentReceivedStatus = ConsentRequest?.ConsentReceivedDate != null && 
            (statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.INRW]
             || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.WITH]
             || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTRW]);
        var isRevokedReviewedStatus = statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTRD] ||
                                      statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.QC] || 
                                      statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.DA];

        if (isConsentReceivedStatus || isRevokedReviewedStatus)
        {
            ShowConsentReceived = true;
        }
    }

    /// <summary>
    /// Determine if to display the Voucher Redemption section.
    /// </summary>
    private void SetVoucherRedemptionState(Guid statusId)
    {
        if (statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.SUB]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REDREV]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.WITHIN]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.QC]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.DA]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REDAPP]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.SENTPAY]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.PAID]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REJPEND]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REJECTED]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.PAYSUS]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REVOKED])
        {
            IsVoucherRedeemed = true;
        }
    }

    /// <summary>
    /// Determine if the cancel application button should be displayed;
    /// </summary>
    private void SetCancelApplicationButtonState(Guid statusId)
    {
        if (statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.INRW]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.WITH]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.QC]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.DA]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VPEND]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VQUED]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTRW]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTRD])
        {
            ShowCancelApplicationButton = true;
        }
    }

    /// <summary>
    /// Determine if the redeem button should be displayed;
    /// </summary>
    private void SetRedeemVoucherButtonState(Guid statusId)
    {
        if (statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VISSD])
        {
            ShowRedeemVoucherButton = true;
        }
    }

    /// <summary>
    /// Determine if the cancel voucher button should be displayed;
    /// </summary>
    private void SetCancelVoucherButtonState(Guid statusId)
    {
        if (statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VISSD])
        {
            ShowCancelVoucherButton = true;
        }
    }

    /// <summary>
    /// Determine if to display the application details section.
    /// </summary>
    private void SetApplicationDetailsState(Guid statusId)
    {
        if (statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.INRW]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.WITH]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.QC]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.DA]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VPEND]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VQUED]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VEXPD]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTRW]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTPS]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTRD]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.RPEND]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.REJECTED]
            || statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.WITHDRAWN]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REVOKED])
        {
            ShowApplicationDetails = true;
        }
    }

    /// <summary>
    /// Determine if to display the voucher details section.
    /// </summary>
    private void SetVoucherDetailsState(Guid statusId)
    {
        if (statusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VISSD])
        {
            ShowVoucherDetails = true;
        }
    }

    /// <summary>
    /// Determine if to display the voucher application details section.
    /// </summary>
    private void SetRedemptionDetailsState(Guid statusId)
    {
        if (statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.SUB]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REDREV]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.WITHIN]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.QC]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.DA]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REDAPP]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.SENTPAY]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.PAID]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REJPEND]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.REJECTED]
            || statusId == StatusMappings.VoucherSubStatus[VoucherSubStatusCode.PAYSUS])
        {
            ShowRedemptionDetails = true;
        }
    }

    private async Task GetApplicationData(string referenceNumber)
    {
        Application = await _applicationsService.GetApplicationByReferenceNumberAsync(referenceNumber);

        if (Application != null)
        {
            _session.ReferenceNumber = Application.ReferenceNumber;
            ConsentRequest = Application.ConsentRequests.OrderByDescending(c => c.CreatedDate).FirstOrDefault();
            CurrentStatusId = Application.Voucher?.VoucherSubStatusID ?? Application.SubStatusId ??
                StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB];
        }
    }

    private async Task GetBusinessAccountData(Guid businessAccountId)
    {
        BusinessAccount = await _businessAccountsService.ExternalGetBusinessAccountAsync(businessAccountId);
    }

    private async Task GetApplicationContacts(Guid businessAccountId, Guid submitterId, Guid? currentContactId)
    {
        var businessAccountUsers = await _businessAccountsService.ExternalGetBusinessAccountUsersAsync(businessAccountId);
        ApplicationSubmitter = businessAccountUsers?.SingleOrDefault(u => u.Id == submitterId);

        if (currentContactId != null)
        {
            var currentContact = businessAccountUsers?.SingleOrDefault(u => u.Id == currentContactId);

            CurrentContactFullName = !string.IsNullOrWhiteSpace(currentContact?.FullName) ? currentContact?.FullName : $"{currentContact?.FirstName} {currentContact?.LastName}";
            CurrentContactEmailAddress = currentContact?.EmailAddress;
        }
        else
        {
            CurrentContactFullName = !string.IsNullOrWhiteSpace(ApplicationSubmitter?.FullName) ? ApplicationSubmitter?.FullName : $"{ApplicationSubmitter?.FirstName} {ApplicationSubmitter?.LastName}";
            CurrentContactEmailAddress = ApplicationSubmitter?.EmailAddress;
        }
    }
}
