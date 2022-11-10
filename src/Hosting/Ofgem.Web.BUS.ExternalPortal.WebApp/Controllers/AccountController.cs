using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Ofgem.API.BUS.BusinessAccounts.Domain.Constants;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using static Ofgem.API.BUS.BusinessAccounts.Domain.Entities.InviteStatus;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Controllers
{
    /// <summary>
    /// Custom Account Controller to handle authentication methods
    /// </summary>
    [AllowAnonymous]
    [Area("CustomIdentity")]
    [Route("[area]/[controller]/[action]")]
    public class AccountController : Controller
    {
        /// <summary>
        /// Identity configuration options
        /// </summary>
        private readonly IOptionsMonitor<MicrosoftIdentityOptions> _optionsMonitor;

        /// <summary>
        /// Application configuration
        /// </summary>
        private readonly IConfiguration _configuration;
        private readonly IGraphApiService _graphApiService;
        private readonly IExternalBusinessAccountService _externalBusinessAccountService;

        /// <summary>
        /// Constructor for Account Controller
        /// </summary>
        /// <param name="microsoftIdentityOptionsMonitor">Identity configuration options</param>
        /// <param name="configuration">Application configuration</param>
        /// <exception cref="ArgumentNullException"></exception>
        public AccountController(IOptionsMonitor<MicrosoftIdentityOptions> microsoftIdentityOptionsMonitor,
            IConfiguration configuration,
            IGraphApiService graphApiService,
            IExternalBusinessAccountService externalBusinessAccountService)
        {
            _optionsMonitor = microsoftIdentityOptionsMonitor;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _graphApiService = graphApiService ?? throw new ArgumentNullException(nameof(graphApiService));
            _externalBusinessAccountService = externalBusinessAccountService ?? throw new ArgumentNullException(nameof(externalBusinessAccountService));
        }

        /// <summary>
        /// Handles user sign in.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <param name="redirectUri">Redirect URI.</param>
        /// <returns>Challenge generating a redirect to Azure AD to sign in the user.</returns>
        [HttpGet("{scheme?}")]
        public IActionResult SignIn(
            [FromRoute] string scheme,
            [FromQuery] string redirectUri)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            string redirect;
            if (!string.IsNullOrEmpty(redirectUri) && Url.IsLocalUrl(redirectUri))
            {
                redirect = redirectUri;
            }
            else
            {
                redirect = Url.Content("~/")!;
            }

            var properties = new AuthenticationProperties { RedirectUri = redirect };
            properties.Items[Constants.Policy] = _configuration.GetValue<string>($"AzureAdB2C:SignUpSignInPolicyId");
            properties.Items["action"] = B2CClaimTypesConstants.SignInAction;

            return Challenge(properties, scheme);
        }

        /// <summary>
        /// Handles user signup.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <param name="redirectUri">Redirect URI.</param>
        /// <param name="registrationEmail">Email to be Signed Up.</param>
        /// <param name="externalUserId">ExternalUserAccountId in BUS DB.</param>
        /// <param name="businessAccountId">BusinessAccountId in BUS DB.</param>
        /// <returns>Challenge generating a redirect to Azure AD to signup.</returns>
        /// <returns></returns>
        [HttpGet("{scheme?}")]
        public IActionResult SignUp(
            [FromRoute] string scheme,
            [FromQuery] string redirectUri,
            [FromQuery] string registrationEmail,
            [FromQuery] string externalUserId,
            [FromQuery] string businessAccountId)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            string redirect;
            if (!string.IsNullOrEmpty(redirectUri) && Url.IsLocalUrl(redirectUri))
            {
                redirect = redirectUri;
            }
            else
            {
                redirect = Url.Content("~/")!;
            }
            if (!User.Identity!.IsAuthenticated)
            {
                var properties = new AuthenticationProperties { RedirectUri = redirect };
                properties.Items[Constants.Policy] = _configuration.GetValue<string>($"AzureAdB2C:SignUpSignInPolicyId");
                properties.Items["action"] = "signup";
                properties.Items["registrationEmail"] = registrationEmail;
                properties.Items["externalUserId"] = externalUserId;
                properties.Items["businessAccountId"] = businessAccountId;

                return Challenge(properties, scheme);
            }
            else
            {
                return SignOut(scheme, "/ApplicationsDashboard/InstallerApplications");
            }
        }

        /// <summary>
        /// Handles user signin first time.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <param name="redirectUri">Redirect URI.</param>
        /// <param name="registrationEmail">Email to be Signed Up.</param>
        /// <param name="externalUserId">ExternalUserAccountId in BUS DB.</param>
        /// <param name="businessAccountId">BusinessAccountId in BUS DB.</param>
        /// <returns>Challenge generating a redirect to Azure AD to signin for first time.</returns>
        /// <returns></returns>
        [HttpGet("{scheme?}")]
        public IActionResult SignInFirstTime(
            [FromRoute] string scheme,
            [FromQuery] string redirectUri,
            [FromQuery] string registrationEmail,
            [FromQuery] string externalUserId,
            [FromQuery] string businessAccountId)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            string redirect;
            if (!string.IsNullOrEmpty(redirectUri) && Url.IsLocalUrl(redirectUri))
            {
                redirect = redirectUri;
            }
            else
            {
                redirect = Url.Content("~/")!;
            }
            if (!User.Identity!.IsAuthenticated)
            {
                var properties = new AuthenticationProperties { RedirectUri = redirect };
                properties.Items[Constants.Policy] = _configuration.GetValue<string>($"AzureAdB2C:SignUpSignInPolicyId");
                properties.Items["action"] = "signinfirsttime";
                properties.Items["registrationEmail"] = registrationEmail;
                properties.Items["externalUserId"] = externalUserId;
                properties.Items["businessAccountId"] = businessAccountId;

                return Challenge(properties, scheme);
            }
            else
            {
                return SignOut(scheme, "/ApplicationsDashboard/InstallerApplications");
            }
        }

        /// <summary>
        /// Handles the user sign-out.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <returns>Sign out result.</returns>
        [HttpGet("{scheme?}")]
        public IActionResult SignOut(
            [FromRoute] string scheme,
            [FromQuery] string redirectUri)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            string redirect = Url.Content("~/signed-out");
            return SignOut(
                 new AuthenticationProperties
                 {
                     RedirectUri = redirect,
                 },
                 CookieAuthenticationDefaults.AuthenticationScheme,
                 scheme);
        }

        /// <summary>
        /// In B2C applications handles the Reset password policy.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <returns>Challenge generating a redirect to Azure AD B2C.</returns>
        [HttpGet("{scheme?}")]
        public IActionResult ResetPassword([FromRoute] string scheme, [FromQuery] string redirectUri)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;

            string redirect;
            if (!string.IsNullOrEmpty(redirectUri) && Url.IsLocalUrl(redirectUri))
            {
                redirect = redirectUri;
            }
            else
            {
                redirect = Url.Content("~/")!;
            }
            var properties = new AuthenticationProperties { RedirectUri = redirect };
            properties.Items[Constants.Policy] = _optionsMonitor.Get(scheme).ResetPasswordPolicyId;
            return Challenge(properties, scheme);
        }

        [AllowAnonymous]
        [HttpGet("/invite")]
        public async Task<IActionResult> Invite(string token)
        {
            var properties = new AuthenticationProperties();
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                properties.RedirectUri = $"/invite?token={token}";
                return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
            }

            var tokenVerificationResult = await _externalBusinessAccountService.VerifyTokenAsync(token);
            if (tokenVerificationResult == null || !tokenVerificationResult.TokenAccepted)
            {
                return RedirectToPage("/B2CRedirect/InviteError");
            }

            var invitation = await _externalBusinessAccountService.GetInviteAsync(tokenVerificationResult.InviteID);
            if (invitation == null || invitation.ExpiresOn.ToUniversalTime() <= DateTime.UtcNow)
            {
                return RedirectToPage("/B2CRedirect/InviteError");
            }

            if(invitation.StatusID != StatusMappings.InviteStatus[InviteStatusCode.INVITED].Id)
            {
                return RedirectToPage("/B2CRedirect/InviteError");
            }

            string inviteEmail = invitation.EmailAddress;

            bool isRegistered = await _graphApiService.HasUserRegistered(inviteEmail);
            if (isRegistered)
            {
                return Authenticate(B2CClaimTypesConstants.SignInFirstTimeAction, B2CClaimTypesConstants.SignInFirstTimeRedirectPath, invitation);
            }

            return Authenticate(B2CClaimTypesConstants.SignUpAction, B2CClaimTypesConstants.SignUpRedirectPath, invitation);
        }

        private IActionResult Authenticate(string authenticationAction, string redirectUri, Invite invitation)
        {
            string scheme = OpenIdConnectDefaults.AuthenticationScheme;
            string redirect;
            if (!string.IsNullOrEmpty(redirectUri) && Url.IsLocalUrl(redirectUri))
            {
                redirect = redirectUri;
            }
            else
            {
                redirect = Url.Content("~/")!;
            }
            if (!User.Identity!.IsAuthenticated)
            {
                var properties = new AuthenticationProperties { RedirectUri = redirect };
                properties.Items[Constants.Policy] = _configuration.GetValue<string>($"AzureAdB2C:SignUpSignInPolicyId");
                properties.Items["action"] = authenticationAction;
                properties.Items["registrationEmail"] = invitation.EmailAddress;
                properties.Items["externalUserId"] = invitation.ExternalUserAccountId.ToString();
                properties.Items["businessAccountId"] = invitation.ExternalUserAccount?.BusinessAccountID.ToString();

                return Challenge(properties, scheme);
            }
            else
            {
                return SignOut(scheme, @Routes.Pages.Path.CD155a);
            }
        }

        /// <summary>
        /// Routing for Unauthorized page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Redirect("/Shared/RoutingError?statusCode=403");
        }
    }
}
