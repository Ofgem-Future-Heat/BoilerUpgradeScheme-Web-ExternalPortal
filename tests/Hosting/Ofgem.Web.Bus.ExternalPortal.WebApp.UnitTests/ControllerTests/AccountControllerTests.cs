using FluentAssertions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Ofgem.API.BUS.BusinessAccounts.Domain.CommsObjects;
using Ofgem.API.BUS.BusinessAccounts.Domain.Constants;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using static Ofgem.API.BUS.BusinessAccounts.Domain.Entities.InviteStatus;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.ControllerTests
{
    [TestFixture]
    public class AccountControllerTests : ControllerTestsBase
    {
        private AccountController? _systemUnderTest;
        private readonly Mock<IOptionsMonitor<MicrosoftIdentityOptions>> _mockMicrosoftIdentityOptions = new();
        private readonly Mock<IGraphApiService> _mockGraphApiService = new();
        private readonly Mock<IExternalBusinessAccountService> _mockExternalBusinessAccountService = new();
        private IConfiguration? _configuration;
        private readonly string _b2cObjectId = Guid.NewGuid().ToString();
        private readonly string _externalUserId = Guid.NewGuid().ToString();
        private readonly string _businessAccountId = Guid.NewGuid().ToString();

        [SetUp]
        public void Setup()
        {
            var inMemoryConfig = new Dictionary<string, string> {
                {"AzureAdB2C:SignUpSignInPolicyId", "B2C_1A_BUS_DEV_SIGNUP_SIGNIN"}
            };
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();
            _systemUnderTest = new AccountController(_mockMicrosoftIdentityOptions.Object, _configuration, _mockGraphApiService.Object, _mockExternalBusinessAccountService.Object);
        }

        [Test]
        public void SignIn_Returns_SuccessfulChallenge()
        {
            // Arrange
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,_b2cObjectId),
                new Claim(B2CClaimTypesConstants.ClaimTypeExternalUserId,_externalUserId),
                new Claim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId,_businessAccountId)
            };

            _systemUnderTest!.ControllerContext.HttpContext = CreateAuthenticatedHttpContext(claims);

            var scheme = OpenIdConnectDefaults.AuthenticationScheme;
            var redirectUri = "/ApplicationsDashboard/InstallerApplications";
            Mock<IUrlHelper> _mockUrl = new();
            _mockUrl.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _systemUnderTest!.Url = _mockUrl.Object;

            // Act
            var result = _systemUnderTest!.SignIn(scheme, redirectUri);

            // Assert
            result.Should().BeOfType<ChallengeResult>();
        }

        [Test]
        public void SignUp_Returns_SuccessfulChallenge()
        {
            // Arrange
            var claims = new List<Claim>();
            _systemUnderTest!.ControllerContext.HttpContext = CreateUnAuthenticatedHttpContext(claims);

            var scheme = OpenIdConnectDefaults.AuthenticationScheme;
            var redirectUri = "/ApplicationsDashboard/InstallerApplications";
            var registrationEmail = "xyz@email.com";
            var externalUserId = Guid.NewGuid().ToString();
            var businessAccountId = Guid.NewGuid().ToString();
            Mock<IUrlHelper> _mockUrl = new();
            _mockUrl.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _systemUnderTest!.Url = _mockUrl.Object;

            // Act
            var result = _systemUnderTest!.SignUp(scheme, redirectUri, registrationEmail, externalUserId, businessAccountId);

            // Assert
            result.Should().BeOfType<ChallengeResult>();
        }

        [Test]
        public void SignInFirstTime_Returns_SuccessfulChallenge()
        {
            // Arrange
            var claims = new List<Claim>();
            _systemUnderTest!.ControllerContext.HttpContext = CreateUnAuthenticatedHttpContext(claims);

            var scheme = OpenIdConnectDefaults.AuthenticationScheme;
            var redirectUri = "/ApplicationsDashboard/InstallerApplications";
            var registrationEmail = "xyz@email.com";
            var externalUserId = Guid.NewGuid().ToString();
            var businessAccountId = Guid.NewGuid().ToString();
            Mock<IUrlHelper> _mockUrl = new();
            _mockUrl.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _systemUnderTest!.Url = _mockUrl.Object;

            // Act
            var result = _systemUnderTest!.SignInFirstTime(scheme, redirectUri, registrationEmail, externalUserId, businessAccountId);

            // Assert
            result.Should().BeOfType<ChallengeResult>();
        }

        [Test]
        public void SignOut_Returns_SuccessfulChallenge()
        {
            // Arrange
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,_b2cObjectId),
                new Claim(B2CClaimTypesConstants.ClaimTypeExternalUserId,_externalUserId),
                new Claim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId,_businessAccountId)
            };
            _systemUnderTest!.ControllerContext.HttpContext = CreateAuthenticatedHttpContext(claims);

            var scheme = OpenIdConnectDefaults.AuthenticationScheme;
            var redirectUri = "/ApplicationsDashboard/InstallerApplications";
            Mock<IUrlHelper> _mockUrl = new();
            _mockUrl.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _systemUnderTest!.Url = _mockUrl.Object;

            // Act
            var result = _systemUnderTest!.SignOut(scheme, redirectUri);

            // Assert
            result.Should().BeOfType<SignOutResult>();
        }

        [Test]
        public void ResetPassword_Returns_SuccessfulChallenge()
        {
            // Arrange
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,_b2cObjectId),
                new Claim(B2CClaimTypesConstants.ClaimTypeExternalUserId,_externalUserId),
                new Claim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId,_businessAccountId)
            };
            _systemUnderTest!.ControllerContext.HttpContext = CreateUnAuthenticatedHttpContext(claims);

            var scheme = OpenIdConnectDefaults.AuthenticationScheme;
            var redirectUri = "/ApplicationsDashboard/InstallerApplications";
            Mock<IUrlHelper> _mockUrl = new();
            _mockUrl.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _systemUnderTest!.Url = _mockUrl.Object;
            var idOptions = new MicrosoftIdentityOptions
            {
                ResetPasswordPolicyId = "B2C_1A_BUS_DEV_PASSWORDRESET"
            };
            _mockMicrosoftIdentityOptions.Setup(m => m.Get(It.IsAny<String>())).Returns(idOptions);

            // Act
            var result = _systemUnderTest!.ResetPassword(scheme, redirectUri);

            // Assert
            result.Should().BeOfType<ChallengeResult>();
        }

        [Test]
        public async Task Invite_InvalidToken_ShouldRedirect()
        {
            _mockExternalBusinessAccountService.Setup(x => x.VerifyTokenAsync(It.IsAny<string>())).Returns(Task.FromResult<TokenVerificationResult>(new() { InviteID = Guid.NewGuid(), TokenAccepted = false }));
            string inviteToken = "TOKEN";

            var result = await _systemUnderTest!.Invite(inviteToken);

            result.Should().BeOfType<RedirectToPageResult>();
            if (result is RedirectToPageResult result1)
            {
                result1.PageName.Should().Be("/B2CRedirect/InviteError");
            }
        }

        [Test]
        public async Task Invite_ExpiredInvite_ShouldRedirect()
        {
            string testEmail = "new_user@ofgem.gov.uk";
            _mockExternalBusinessAccountService.Setup(x => x.VerifyTokenAsync(It.IsAny<string>())).Returns(Task.FromResult<TokenVerificationResult>(new() { InviteID = Guid.NewGuid(), TokenAccepted = true }));
            _mockExternalBusinessAccountService.Setup(x => x.GetInviteAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Invite>(new() { EmailAddress = testEmail, ExpiresOn = DateTime.UtcNow.AddDays(-1), StatusID = StatusMappings.InviteStatus[InviteStatusCode.INVITED].Id }));
            string inviteToken = "TOKEN";

            var result = await _systemUnderTest!.Invite(inviteToken);

            result.Should().BeOfType<RedirectToPageResult>();
            if (result is RedirectToPageResult result1)
            {
                result1.PageName.Should().Be("/B2CRedirect/InviteError");
            }
        }

        [Test]
        public async Task Invite_ExistingB2cUser_ShouldRedirect()
        {
            var claims = new List<Claim>();
            _systemUnderTest!.ControllerContext.HttpContext = CreateUnAuthenticatedHttpContext(claims);

            string testEmail = "new_user@ofgem.gov.uk";
            Guid testExternalUserAccountId = Guid.NewGuid();
            Guid testBusinessAccountId = Guid.NewGuid();
            _mockExternalBusinessAccountService.Setup(x => x.VerifyTokenAsync(It.IsAny<string>())).Returns(Task.FromResult<TokenVerificationResult>(new() { InviteID = Guid.NewGuid(), TokenAccepted = true }));
            _mockExternalBusinessAccountService.Setup(x => x.GetInviteAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Invite>(new() { EmailAddress = testEmail, ExpiresOn = DateTime.UtcNow.AddDays(1), StatusID = StatusMappings.InviteStatus[InviteStatusCode.INVITED].Id, ExternalUserAccountId = testExternalUserAccountId, ExternalUserAccount = new() { BusinessAccountID = testBusinessAccountId } }));
            _mockGraphApiService.Setup(x => x.HasUserRegistered(testEmail)).Returns(Task.FromResult(true));
            string inviteToken = "TOKEN";
            Mock<IUrlHelper> _mockUrl = new();
            _mockUrl.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _systemUnderTest!.Url = _mockUrl.Object;

            var result = await _systemUnderTest!.Invite(inviteToken);

            result.Should().BeOfType<ChallengeResult>();
            if (result is ChallengeResult result1)
            {
                result1?.Properties?.Items.ContainsKey("action").Should().Be(true);
                result1?.Properties?.Items["action"].Should().Be(B2CClaimTypesConstants.SignInFirstTimeAction);

                result1?.Properties?.Items.ContainsKey("registrationEmail").Should().Be(true);
                result1?.Properties?.Items["registrationEmail"].Should().Be(testEmail);

                result1?.Properties?.Items.ContainsKey("externalUserId").Should().Be(true);
                result1?.Properties?.Items["externalUserId"].Should().Be(testExternalUserAccountId.ToString());

                result1?.Properties?.Items.ContainsKey("businessAccountId").Should().Be(true);
                result1?.Properties?.Items["businessAccountId"].Should().Be(testBusinessAccountId.ToString());
            }
        }

        [Test]
        public async Task Invite_NewUser_ShouldChallenge()
        {
            var claims = new List<Claim>();
            _systemUnderTest!.ControllerContext.HttpContext = CreateUnAuthenticatedHttpContext(claims);

            string testEmail = "new_user@ofgem.gov.uk";
            Guid testExternalUserAccountId = Guid.NewGuid();
            Guid testBusinessAccountId = Guid.NewGuid();
            _mockExternalBusinessAccountService.Setup(x => x.VerifyTokenAsync(It.IsAny<string>())).Returns(Task.FromResult<TokenVerificationResult>(new() { InviteID = Guid.NewGuid(), TokenAccepted = true }));
            _mockExternalBusinessAccountService.Setup(x => x.GetInviteAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Invite>(new() { EmailAddress = testEmail, ExpiresOn = DateTime.UtcNow.AddDays(1), StatusID = StatusMappings.InviteStatus[InviteStatusCode.INVITED].Id,  ExternalUserAccountId = testExternalUserAccountId, ExternalUserAccount = new() { BusinessAccountID = testBusinessAccountId } }));
            _mockGraphApiService.Setup(x => x.HasUserRegistered(testEmail)).Returns(Task.FromResult(false));
            string inviteToken = "TOKEN";
            Mock<IUrlHelper> _mockUrl = new();
            _mockUrl.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _systemUnderTest!.Url = _mockUrl.Object;

            var result = await _systemUnderTest!.Invite(inviteToken);

            result.Should().BeOfType<ChallengeResult>();
            if (result is ChallengeResult result1)
            {
                result1?.Properties?.Items.ContainsKey("action").Should().Be(true);
                result1?.Properties?.Items["action"].Should().Be(B2CClaimTypesConstants.SignUpAction);

                result1?.Properties?.Items.ContainsKey("registrationEmail").Should().Be(true);
                result1?.Properties?.Items["registrationEmail"].Should().Be(testEmail);

                result1?.Properties?.Items.ContainsKey("externalUserId").Should().Be(true);
                result1?.Properties?.Items["externalUserId"].Should().Be(testExternalUserAccountId.ToString());

                result1?.Properties?.Items.ContainsKey("businessAccountId").Should().Be(true);
                result1?.Properties?.Items["businessAccountId"].Should().Be(testBusinessAccountId.ToString());
            }
        }

        [Test]
        public async Task Invite_NewUser_SignedIn_ShouldSignOut()
        {
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(x => x!.User!.Identity!.IsAuthenticated).Returns(true);
            var controllerContext = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            _systemUnderTest!.ControllerContext = controllerContext;
            string inviteToken = "TOKEN";

            var result = await _systemUnderTest!.Invite(inviteToken);

            result.Should().BeOfType<SignOutResult>();
            if (result is SignOutResult result1)
            {
                result1?.Properties?.RedirectUri.Should().Be($"/invite?token={inviteToken}");
            }
        }

        [Test]
        public void AccessDenied_Returns_Success()
        {
            // Arrange
            var expected = new RedirectResult("/Shared/RoutingError?statusCode=403");

            // Act
            var result = _systemUnderTest!.AccessDenied();

            // Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}
