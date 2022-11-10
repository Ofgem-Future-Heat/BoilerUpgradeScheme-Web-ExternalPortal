using Microsoft.AspNetCore.Mvc;
using Ofgem.API.BUS.Applications.Domain.Constants;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using static Ofgem.API.BUS.Applications.Domain.ApplicationSubStatus;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.CheckAnswers;

[PageHistory]
public class CheckYourAnswersModel : AbstractFormPage
{
    private readonly IEnumerable<Guid> _invalidBusinessAccountStatuses = new List<Guid>
    {
        API.BUS.BusinessAccounts.Domain.Constants.StatusMappings.BusinessAccountSubStatus[BusinessAccountSubStatus.BusinessAccountSubStatusCode.FAIL].Id,
        API.BUS.BusinessAccounts.Domain.Constants.StatusMappings.BusinessAccountSubStatus[BusinessAccountSubStatus.BusinessAccountSubStatusCode.REVOK].Id,
        API.BUS.BusinessAccounts.Domain.Constants.StatusMappings.BusinessAccountSubStatus[BusinessAccountSubStatus.BusinessAccountSubStatusCode.WITHDR].Id
    };

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<CheckYourAnswersModel> _logger;
    private readonly IExternalApplicationsService _applicationsService;
    private readonly IExternalBusinessAccountService _businessAccountService;
    private readonly SessionService _sessionService;

    public CreateApplicationModel CreateApplication { get; set; } = null!;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public CheckYourAnswersModel(ILogger<CheckYourAnswersModel> logger, IExternalApplicationsService applicationsService, IExternalBusinessAccountService businessAccountService, SessionService sessionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));
        _businessAccountService = businessAccountService ?? throw new ArgumentNullException(nameof(businessAccountService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnGet()
    {
        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        CreateApplication = sessionModel;

        return Page();
    }

    /// <summary>
    /// OnPostAsync - Gets the user decision for the next action.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogDebug("CheckYourAnswersModel -> OnPostAsync");

        var user = await _businessAccountService.GetExternalUserAccountByIdAsync(_sessionService.UserId!.Value);
        var businessAccount = await _businessAccountService.ExternalGetBusinessAccountAsync(_sessionService.BusinessAccountId);

        if (UserOrBusinessAccountNotActive(user, businessAccount))
        {
            return RedirectToPage(@Routes.Pages.Path.CD187);
        }

        if (!ModelState.IsValid)
        {
            return OnGet();
        }

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        try
        {
            var createdApplication = await _applicationsService.CreateApplicationAsync(sessionModel, User);

            if (createdApplication != null)
            {
                if (createdApplication.SubStatusId == StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB])
                {
                    _ = await _applicationsService.CreateVoucherAsync(createdApplication.ID, sessionModel.TechTypeId!.Value, User);

                    if (sessionModel.CanConsentOnline == true
                        && sessionModel.WelshConsent == false
                        && !string.IsNullOrEmpty(sessionModel.InstallationAddress!.UPRN))
                    {
                        _ = await _applicationsService.SendConsentEmailAsync(createdApplication.ID, User);
                    }
                }

                ClearPageModel();
                ClearPageHistory();

                TempData.Put("ApplicationConfirmationDataModel", new ApplicationConfirmationModel
                {
                    ApplicationId = createdApplication.ID,
                    ApplicationReferenceNumber = createdApplication.ReferenceNumber,
                    InstallerEmailAddress = _sessionService.UserEmail,
                    PropertyOwnerEmailAddress = sessionModel.PropertyOwnerEmailAddress,
                    EpcHasExemptions = sessionModel.EpcHasExemptions.HasValue && sessionModel.EpcHasExemptions == true,
                    IsEligibleSelfBuild = sessionModel.IsEligibleSelfBuild.HasValue && sessionModel.IsEligibleSelfBuild == true,
                    IsWelshConsent = sessionModel.WelshConsent.HasValue && sessionModel.WelshConsent == true,
                    IsManualConsent = (sessionModel.WelshConsent.HasValue && sessionModel.WelshConsent == true) || (sessionModel.CanConsentOnline.HasValue && sessionModel.CanConsentOnline == false)
                    
                });

                return RedirectToPage(@Routes.Pages.Path.CD026);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new application");
            
            ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
        }

        return OnGet();
    }

    private bool UserOrBusinessAccountNotActive(ExternalUserAccount user, BusinessAccount businessAccount)
    {
        if (user == null || user.IsObsolete)
            return true;

        if (businessAccount == null || businessAccount.ActiveDate == null ||
            _invalidBusinessAccountStatuses.Contains(businessAccount.SubStatusId))
            return true;

        return false;
    }
}
