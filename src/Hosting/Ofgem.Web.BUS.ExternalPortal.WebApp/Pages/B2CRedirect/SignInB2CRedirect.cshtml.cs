using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using System.Security.Claims;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.B2CRedirect
{
    public class SignInB2CRedirectModel : PageModel
    {
        /// <summary>
        /// Logging for instumentation of code
        /// </summary>
        private readonly ILogger<SignInB2CRedirectModel> _logger;

        /// <summary>
        /// Interact with business client
        /// </summary>
        private readonly IExternalBusinessAccountService _businessAccountService;

        /// <summary>
        /// Session handler
        /// </summary>
        private SessionService _sessionService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="externalBusinessAccountService">Business account service endpoint.</param>
        /// <param name="logger">Application logging.</param>
        /// <exception cref="ArgumentNullException">Exception for null arguments.</exception>
        public SignInB2CRedirectModel(
            IExternalBusinessAccountService externalBusinessAccountService,
            SessionService sessionService,
            ILogger<SignInB2CRedirectModel> logger)
        {
            _sessionService = sessionService;
            _businessAccountService = externalBusinessAccountService ?? throw new ArgumentNullException(nameof(externalBusinessAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles Get request. Checks for Obsolete user & redirects appropriately.
        /// </summary>
        /// <returns>Page redirect.</returns>
        public async Task<IActionResult> OnGet()
        {
            _logger.LogInformation("public IActionResult OnGet");
            try
            {
                if (User.Identity!.IsAuthenticated)
                {
                    var externalUserAccountId = User.FindFirstValue(B2CClaimTypesConstants.ClaimTypeExternalUserId);
                    var externalBusinessAccountId = User.FindFirstValue(B2CClaimTypesConstants.ClaimTypeBusinessAccountId);
                    var externalUserEmail = User.FindFirstValue(B2CClaimTypesConstants.ClaimTypeEmailAddress);

                    if (string.IsNullOrWhiteSpace(externalBusinessAccountId) || string.IsNullOrWhiteSpace(externalUserAccountId))
                    {
                        throw new ArgumentNullException();
                    }

                    var externalUserAccountIdGuid = Guid.Parse(externalUserAccountId);
                    var externalBusAccountIdGuid = Guid.Parse(externalBusinessAccountId);

                    var externalUserAccount = await _businessAccountService.GetExternalUserAccountByIdAsync(externalUserAccountIdGuid);

                    if (externalBusAccountIdGuid != externalUserAccount?.BusinessAccountID)
                    {
                        throw new NullReferenceException();
                    }

                    HttpContext.Session.Put(B2CConstants.IsObsoleteUserKey, externalUserAccount.IsObsolete);

                    if (!externalUserAccount.IsObsolete)
                    {
                        _sessionService.BusinessAccountId = externalBusAccountIdGuid;
                        _sessionService.UserId = externalUserAccountIdGuid;
                        _sessionService.UserEmail = externalUserEmail;

                        var prevPage = HttpContext.Session.GetOrDefault<string>(B2CConstants.PageRedirectKey) ?? String.Empty;

                        if (prevPage.Contains($"{Routes.CD149}?"))
                        {
                            return Redirect(prevPage);
                        }

                        return RedirectToPage(@Routes.Pages.Path.CD155a);
                    }
                }
                return Redirect(B2CConstants.AccessDeniedPath);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Business account ID for External User does not match the claim.");
                HttpContext.Session.Put(B2CConstants.PageRedirectKey, "/Shared/InternalError");
                return RedirectToPage("/Shared/InternalError");
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "One or more B2C claims for user and business account are null or empty.");
                HttpContext.Session.Put(B2CConstants.PageRedirectKey, "/Shared/InternalError");
                return RedirectToPage("/Shared/InternalError");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in checking obsolete user after signin.");
                HttpContext.Session.Put(B2CConstants.PageRedirectKey, "/Shared/InternalError");
                return RedirectToPage("/Shared/InternalError");
            }
        }
    }
}
