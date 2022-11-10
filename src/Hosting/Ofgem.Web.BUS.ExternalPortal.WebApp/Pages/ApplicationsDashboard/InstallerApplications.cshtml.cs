using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Microsoft.Graph.SecurityNamespace;
using Ofgem.API.BUS.Applications.Domain.Constants;
using Ofgem.API.BUS.Applications.Domain.Entities.Enums;
using Ofgem.API.BUS.Applications.Domain.Entities.Views;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using Ofgem.Web.BUS.ExternalPortal.Domain.DTOs;
using static Ofgem.API.BUS.Applications.Domain.ApplicationStatus;
using static Ofgem.API.BUS.Applications.Domain.VoucherStatus;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationsDashboard;

public class InstallerApplicationsModel : PageModel
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
    private readonly ILogger<InstallerApplicationsModel> _logger;

    /// <summary>
    /// Data from the Business Account API to retrieve account data and validation.
    /// </summary>
    public BusinessAccount? BusinessAccountData { get; set; }
    
    public bool IsAllowedCreateNewCase { get; set; } = false;

    public IEnumerable<ExternalPortalDashboardApplication> Applications { get; set; } = Enumerable.Empty<ExternalPortalDashboardApplication>();

    /// <summary>
    /// Filter and Search by string value of user defined input actions.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? SearchBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedFilterKey { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedFilterValue { get; set; }

    /// <summary>
    /// Filter of predefined items to filter by.
    /// </summary>
    public SelectList? FilterSelectListItems { get; set; }

    /// <summary>
    /// Session handler for user interactions.
    /// </summary>
    private readonly SessionService _session;

    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <param name="businessAccountsService">Business Account Service end point.</param>
    /// <param name="applicationsService">Application Service end point.</param>
    /// <param name="session">Session for local user retained values.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public InstallerApplicationsModel(ILogger<InstallerApplicationsModel> logger,
                                       IExternalBusinessAccountService businessAccountsService,
                                       IExternalApplicationsService applicationsService,
                                       SessionService session,
                                       IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _businessAccountsService = businessAccountsService ?? throw new ArgumentNullException(nameof(businessAccountsService));
        _applicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _configuration = configuration;
    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <param name="searchBy">The passed business account id.</param>
    /// <param name="selectedFilterKey">The passed business account id.</param>
    /// <param name="selectedFilterValue">The passed business account id.</param>
    /// <param name="flush">Flush the session for filter/search.</param>
    /// <returns>IAction result to display page. GRP implementation</returns>
    public async Task<IActionResult> OnGetAsync(string? searchBy, string? selectedFilterKey, string? selectedFilterValue, bool? flush = false)
    {
        _logger.LogTrace("InstallerApplicationsModel -> OnGet");

        bool flushTheSessionFilters = flush ?? false;
        if (flushTheSessionFilters)
        {
            _session.SearchBy = string.Empty;
            _session.SelectedFilterValue = string.Empty;
            _session.SelectedFilterKey = string.Empty;
        }
        else
        {
            _session.SearchBy = searchBy ?? string.Empty;
            _session.SelectedFilterValue = selectedFilterValue ?? string.Empty;
            _session.SelectedFilterKey = selectedFilterKey ?? string.Empty;
        }

        FilterSelectListItems = PopulateFilterListItems();

        BusinessAccountData = await _businessAccountsService.ExternalGetBusinessAccountAsync(_session.BusinessAccountId);

        /* no valid business account data found go to error page */
        if (BusinessAccountData == null || BusinessAccountData.Id == Guid.Empty)
        {
            return RedirectToPage("/Shared/InternalError");
        }

        switch (BusinessAccountData.SubStatus?.Code)
        {
            case BusinessAccountSubStatus.BusinessAccountSubStatusCode.FAIL:
            case BusinessAccountSubStatus.BusinessAccountSubStatusCode.REVOK:
            case BusinessAccountSubStatus.BusinessAccountSubStatusCode.WITHDR:
                IsAllowedCreateNewCase = false;
                break;
            default:
                IsAllowedCreateNewCase = true;
                break;
        }

        if (BusinessAccountData.ActiveDate == null)
        {
            IsAllowedCreateNewCase = false;
        }

        _session.IsAllowedCreateNewCase = IsAllowedCreateNewCase;

        SearchBy = _session.SearchBy;
        SelectedFilterValue = _session.SelectedFilterValue;
        SelectedFilterKey = _session.SelectedFilterKey;

        var filterStatusIds = Enumerable.Empty<Guid>();
        string filterConsentState = string.Empty;

        if (!string.IsNullOrWhiteSpace(SelectedFilterKey))
        {
            var filterItems = GetFilterListItems();
            var filterItem = filterItems.FirstOrDefault(item => item.Key == SelectedFilterKey);

            if (filterItem?.StatusIds != null)
            {
                filterStatusIds = filterItem.StatusIds;
            }
            else if (filterItem?.ConsentState != null)
            {
                filterConsentState = filterItem.ConsentState.ToString();
            }
        }

        Applications = await _applicationsService.GetDashboardApplicationsByBusinessAccountIdAsync(_session.BusinessAccountId!.Value, _session.SearchBy, filterStatusIds, filterConsentState);

        return Page();
    }

    /// <summary>
    /// Post the form to initialises all filter values(s).
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    /// GRP implementation
    public IActionResult OnPostApplySearchAndFilters(IFormCollection data)
    {
        _logger.LogTrace("InstallerApplicationsModel -> OnPostApplySearchAndFilters");

        if (data.TryGetValue("status-dropdown", out StringValues selectedFilterStringValues))
        {
            SelectedFilterKey = selectedFilterStringValues.First();
        }

        FilterSelectListItems = PopulateFilterListItems();
        if (!string.IsNullOrEmpty(SelectedFilterKey))
        {
            SelectedFilterValue = FilterSelectListItems.First(k => k.Value == SelectedFilterKey).Text;
        }

        if (SearchBy?.Length > 200)
        {
            return RedirectToPage("");
        }

        return RedirectToPage("", new
        {   
            searchBy = SearchBy,
            selectedFilterValue = SelectedFilterValue,
            selectedFilterKey = SelectedFilterKey
        });
    }

    private static IEnumerable<DashboardFilterListItem> GetFilterListItems()
    {
        var returnList = new List<DashboardFilterListItem>
        {
            new DashboardFilterListItem {Key = string.Empty, Value = "Show all"},
            new DashboardFilterListItem
            {
                Key ="review",
                Value = "Reviewing application",
                StatusIds = new List<Guid>
                {
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.SUB],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.INRW],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.CNTRW],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.VPEND]
                }
            },
            new DashboardFilterListItem { Key = "notissued", Value = "Consent not yet issued", ConsentState = ConsentState.NotIssued },
            new DashboardFilterListItem
            {
                Key = "pendingconsent",
                Value = "Remind owner to consent",
                StatusIds = new List<Guid>
                {
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.INRW],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.WITH],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.CNTRW]
                }
            },
            new DashboardFilterListItem{ Key = "consentreceived", Value = "Consent received", ConsentState = ConsentState.Received },
            new DashboardFilterListItem
            {
                Key = "action",
                Value = "Reply to Ofgem",
                StatusIds = new List<Guid>
                {
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.WITH],
                    StatusMappings.VoucherStatus[VoucherStatusCode.WITHIN]
                }
            },
            new DashboardFilterListItem
            {
                Key = "voucherqueued",
                Value = "Voucher queued",
                StatusIds = new List<Guid>{ StatusMappings.ApplicationStatus[ApplicationStatusCode.VQUED] }
            },
            new DashboardFilterListItem
            {
                Key = "voucherissued",
                Value = "Voucher issued",
                StatusIds = new List<Guid>{ StatusMappings.ApplicationStatus[ApplicationStatusCode.VISSD] }
            },
            new DashboardFilterListItem
            {
                Key = "ReviewingRedemption",
                Value = "Reviewing redemption",
                StatusIds = new List<Guid>
                {
                    StatusMappings.VoucherStatus[VoucherStatusCode.REDREC],
                    StatusMappings.VoucherStatus[VoucherStatusCode.REDREV],
                    StatusMappings.VoucherStatus[VoucherStatusCode.REDAPP],
                    StatusMappings.VoucherStatus[VoucherStatusCode.SENTPAY],
                    StatusMappings.VoucherStatus[VoucherStatusCode.PAYSUS]
                }
            },
            new DashboardFilterListItem
            {
                Key = "Paid",
                Value = "Paid",
                StatusIds = new List<Guid>
                {
                    StatusMappings.VoucherStatus[VoucherStatusCode.PAID]
                }
            },
            new DashboardFilterListItem
            {
                Key = "Rejected",
                Value = "Rejected",
                StatusIds = new List<Guid>
                {
                    StatusMappings.VoucherStatus[VoucherStatusCode.REJECTED],
                    StatusMappings.VoucherStatus[VoucherStatusCode.REJPEND],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.VEXPD],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.CNTPS],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.REJECTED],
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.RPEND]
                }
            },
            new DashboardFilterListItem
            {
                Key = "Cancelled",
                Value = "Cancelled",
                StatusIds = new List<Guid>
                {
                    StatusMappings.ApplicationStatus[ApplicationStatusCode.WITHDRAWN]
                }
            },
            new DashboardFilterListItem
            {
                Key = "Revoked",
                Value = "Revoked",
                StatusIds = new List<Guid>
                {
                    StatusMappings.VoucherStatus[VoucherStatusCode.REVOKED]
                }
            }
        };

        return returnList;
    }

    /// <summary>
    /// Gets a list of statuses for the status dropdown.
    /// </summary>
    /// <returns>SelectList of filter key/value pair.</returns>
    private static SelectList PopulateFilterListItems()
    {
        var selectListItems = GetFilterListItems();

        return new SelectList(selectListItems, "Key", "Value");
    }
}