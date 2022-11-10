using Microsoft.AspNetCore.Mvc;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Utilities;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.Security.Claims;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.B2CRedirect
{
    public class SignUpB2CRedirectModel : AbstractFormPage
    {
        /// <summary>
        /// Logging for instumentation of code
        /// </summary>
        private readonly ILogger<SignUpB2CRedirectModel> _logger;

        /// <summary>
        /// Interact with business client
        /// </summary>
        private readonly IExternalBusinessAccountService _businessAccountService;

        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Application logging.</param>
        /// <param name="businessAccountService">Business account service endpoint.</param>
        /// <exception cref="ArgumentNullException">Exception for null arguments.</exception>
        public SignUpB2CRedirectModel(ILogger<SignUpB2CRedirectModel> logger,
                                      IExternalBusinessAccountService businessAccountService,
                                      IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _businessAccountService = businessAccountService ?? throw new ArgumentNullException(nameof(businessAccountService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// OnGet - Signup claim validation, Save B2C details to BUS & Redirect.
        /// </summary>
        /// <returns>Page redirect</returns>
        public async Task<IActionResult> OnGet()
        {
            _logger.LogInformation("public IActionResult OnGet()");
            try
            {
                if (User.Identity!.IsAuthenticated)
                {
                    string b2cObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    string externalUserId = User.FindFirstValue(B2CClaimTypesConstants.ClaimTypeExternalUserId);
                    string businessAccountId = User.FindFirstValue(B2CClaimTypesConstants.ClaimTypeBusinessAccountId);
                    string action = User.FindFirstValue(B2CClaimTypesConstants.ClaimTypeAction);

                    if (!B2CUtilities.IsValidAdb2cId(b2cObjectId))
                    {
                        throw new ArgumentNullException(nameof(b2cObjectId));
                    }
                    else if (!B2CUtilities.IsValidBusinessAccountId(businessAccountId))
                    {
                        throw new ArgumentNullException(nameof(businessAccountId));
                    }
                    else if (!B2CUtilities.IsValidExternalUserId(externalUserId))
                    {
                        throw new ArgumentNullException(nameof(externalUserId));
                    }
                    else
                    {
                        var externalUserAccount = await _businessAccountService.GetExternalUserAccountByIdAsync(Guid.Parse(externalUserId));
                        externalUserAccount.AADB2CId = Guid.Parse(b2cObjectId);
                        externalUserAccount.LastUpdatedBy = User.GetFullName();
                        var _now = DateTime.UtcNow;
                        externalUserAccount.LastUpdatedDate = _now;
                        externalUserAccount.TermsLastAcceptedDate = _now;
                        externalUserAccount.TermsLastAcceptedVersion = int.Parse(_config["AzureAdB2C:TermsConditionsVersion"]);

                        List<ExternalUserAccount> updatedExternalUserAccounts = new()
                        {
                            externalUserAccount
                        };
                        var updateResult = await _businessAccountService.UpdateExternalUserAccountsAsync(updatedExternalUserAccounts, Guid.Parse(businessAccountId));

                        var invites = await _businessAccountService.GetInvitesForUserAsync(Guid.Parse(externalUserId));
                        var activeInvites = invites.Where(x => x.Status!.Code == InviteStatus.InviteStatusCode.INVITED).ToList();

                        var inviteStatuses = await _businessAccountService.GetAllInviteStatusAsync();

                        activeInvites.ForEach(async x =>
                        {
                            var signedUpStatus = inviteStatuses.Where(i => i.Code == InviteStatus.InviteStatusCode.SIGNEDUP).FirstOrDefault(x.Status);
                            x.Status = signedUpStatus;
                            x.StatusID = signedUpStatus!.Id;
                            await _businessAccountService.UpdateInviteAsync(x, x.ID);
                        });
                        if (action.Equals(B2CClaimTypesConstants.SignUpAction, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return Page();
                        }
                        else
                        {
                            return RedirectToPage(@Routes.Pages.Path.CD155a);
                        }
                    }
                }
                else
                {
                    return RedirectToPage("/Shared/InternalError");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve B2CId/ExternalUserId/BusinessAccountId claims from ClaimsPrincipal");
                return RedirectToPage("/Shared/InternalError");
            }
        }

        /// <summary>
        /// OnPost - gets the user response go back home!
        /// </summary>
        /// <returns>IAction result to display page.</returns>
        public IActionResult OnPostHomeButton()
        {
            _logger.LogInformation("public IActionResult OnPost()");
            return Redirect("~/ApplicationsDashboard/InstallerApplications");
        }
    }
}
