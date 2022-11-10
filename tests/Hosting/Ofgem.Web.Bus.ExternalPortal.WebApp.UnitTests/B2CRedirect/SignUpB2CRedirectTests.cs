using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.B2CRedirect;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.B2CRedirect
{
    [TestFixture]
    public class SignUpB2CRedirectTests : PageModelTestsBase
    {
        private SignUpB2CRedirectModel _systemUnderTest;
        private Mock<ILogger<SignUpB2CRedirectModel>> _mockLogger = new();
        private Mock<IExternalBusinessAccountService> _mockBusinessAccountService = new();
        private Mock<IConfiguration> _mockConfig = new();
        private readonly string _b2cObjectId = Guid.NewGuid().ToString();
        private readonly string _externalUserId = Guid.NewGuid().ToString();
        private readonly string _businessAccountId = Guid.NewGuid().ToString();
        private HttpContext _httpContext;
        private ExternalUserAccount _externalUserAccount = new();
        private List<InviteStatus> _inviteStatuses = new();
        private List<Invite> _invites = new();

        [SetUp]
        public override void TestCaseSetUp()
        {
            _systemUnderTest = new SignUpB2CRedirectModel(_mockLogger.Object, _mockBusinessAccountService.Object, _mockConfig.Object);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,_b2cObjectId),
                new Claim(B2CClaimTypesConstants.ClaimTypeExternalUserId,_externalUserId),
                new Claim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId,_businessAccountId),
                new Claim(B2CClaimTypesConstants.ClaimTypeAction, B2CClaimTypesConstants.SignInAction)
            };
            var claimsIdentity = new ClaimsIdentity("custom_auth");
            claimsIdentity.AddClaims(claims);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(claimsIdentity);
            _httpContext = new DefaultHttpContext
            {
                Session = Session,
                User = claimsPrincipal
            };
            _externalUserAccount = new ExternalUserAccount
            {
                Id = Guid.Parse(_externalUserId),
                AuthorisedRepresentative = true,
                FirstName = "Authorised",
                LastName = "Rep",
                EmailAddress = "AuthorisedTest@Ofgem.gov.uk",
                TelephoneNumber = "01234567890",
                HomeAddress = "2 Test Street",
                HomeAddressPostcode = "TE22 2ST",
                HomeAddressUPRN = "098765432100",
                DOB = new DateTime(1997, 11, 17)
            };
            _inviteStatuses = new List<InviteStatus>
                {
                      new InviteStatus{
                        Id= Guid.Parse("1f705c16-74c3-4b46-b3db-48c267beba49"),
                        Code= 0,
                        DisplayName= "Invite Not Sent",
                        Description= "Invite Not Sent"
                      },
                      new InviteStatus{
                        Id= Guid.Parse( "1a853f93-94ce-4ac5-938c-5a943e1fd0f7"),
                        Code= (InviteStatus.InviteStatusCode)5,
                        DisplayName= "Invite Cancelled",
                        Description= "Invite Cancelled"
                      },
                      new InviteStatus{
                        Id= Guid.Parse( "eb989258-b1fe-4733-885d-98f96e95b31c"),
                        Code= (InviteStatus.InviteStatusCode)3,
                        DisplayName=  "Invite Expired",
                         Description=  "Invite Expired"
                      },
                      new InviteStatus{
                       Id= Guid.Parse("9ff91ce7-6a54-47cf-980b-a28687f0ddc0"),
                        Code= (InviteStatus.InviteStatusCode)4,
                      DisplayName= "Signed Up",
                         Description=  "Signed Up"
                      },
                      new InviteStatus{
                        Id= Guid.Parse("d8a3632d-2629-43cc-8bfd-b198db6b97ab"),
                        Code= (InviteStatus.InviteStatusCode)1,
                       DisplayName= "Invited",
                        Description=  "Invited"
                      },
                      new InviteStatus{
                          Id = Guid.Parse("d08080c0-efa3-4e21-be39-daec74a0bb56"),
                          Code = (InviteStatus.InviteStatusCode)2,
                          DisplayName = "Invite Not Delivered",
                         Description=  "Invite Not Delivered"
                      }
                };
            _invites = new List<Invite> {
                new Invite{
                    ID = Guid.NewGuid(),
                    ExternalUserAccountId = _externalUserAccount.Id,
                    AccountName = _externalUserAccount.FirstName + " " + _externalUserAccount.LastName,
                    FullName = _externalUserAccount.FirstName + " " + _externalUserAccount.LastName,
                    EmailAddress = _externalUserAccount.EmailAddress,
                    SentOn = DateTime.Now,
                    ExpiresOn = DateTime.Now.AddDays(7),
                    Status = new InviteStatus{
                        Id= Guid.Parse("d8a3632d-2629-43cc-8bfd-b198db6b97ab"),
                        Code= (InviteStatus.InviteStatusCode)1,
                        DisplayName= "Invited",
                        Description=  "Invited"
                      },
                    StatusID = Guid.Parse("d8a3632d-2629-43cc-8bfd-b198db6b97ab")
                }
            };
            base.TestCaseSetUp();
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
        {
            // Arrange and Act.
            var action = () => new SignUpB2CRedirectModel(null!, _mockBusinessAccountService.Object, _mockConfig.Object);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentNullException_If_BusinessAccountService_Null()
        {
            // Arrange and Act.
            var action = () => new SignUpB2CRedirectModel(_mockLogger.Object, null!, _mockConfig.Object);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("businessAccountService");
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentNullException_If_Configuration_Null()
        {
            // Arrange and Act.
            var action = () => new SignUpB2CRedirectModel(_mockLogger.Object, _mockBusinessAccountService.Object, null!);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("config");
        }

        [Test]
        public async Task OnGet_RedirectsTo_SignUpSuccess_When_NewUserProvided()
        {
            // Arrange
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,_b2cObjectId),
                new Claim(B2CClaimTypesConstants.ClaimTypeExternalUserId,_externalUserId),
                new Claim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId,_businessAccountId),
                new Claim(B2CClaimTypesConstants.ClaimTypeAction, B2CClaimTypesConstants.SignUpAction)
            };
            var claimsIdentity = new ClaimsIdentity("custom_auth");
            claimsIdentity.AddClaims(claims);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(claimsIdentity);
            _httpContext = new DefaultHttpContext
            {
                Session = Session,
                User = claimsPrincipal
            };
            _systemUnderTest.PageContext = CreatePageContext(_httpContext);
            _mockBusinessAccountService.Setup(m => m.GetExternalUserAccountByIdAsync(It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(_externalUserAccount));
            var updatedAccounts = new List<ExternalUserAccount>() { _externalUserAccount };
            _mockBusinessAccountService.Setup(m => m.UpdateExternalUserAccountsAsync(It.IsAny<List<ExternalUserAccount>>(), It.IsAny<Guid>())).Returns(Task.FromResult(updatedAccounts));
            _mockBusinessAccountService.Setup(m => m.GetInvitesForUserAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_invites));
            _mockBusinessAccountService.Setup(m => m.GetAllInviteStatusAsync()).Returns(Task.FromResult(_inviteStatuses));
            _mockBusinessAccountService.Setup(m => m.UpdateInviteAsync(It.IsAny<Invite>(), It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Invite>()));
            _mockConfig.Setup(m => m["AzureAdB2C:TermsConditionsVersion"]).Returns("20221005");

            // Act
            var result = await _systemUnderTest.OnGet();

            // Assert
            using (new AssertionScope())
            {
                Assert.IsNotNull(result);
                result.Should().BeOfType<PageResult>();
            };
        }

        [Test]
        public async Task OnGet_RedirectsTo_Dashboard_When_ExistingUserProvided()
        {
            // Arrange
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,_b2cObjectId),
                new Claim(B2CClaimTypesConstants.ClaimTypeExternalUserId,_externalUserId),
                new Claim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId,_businessAccountId),
                new Claim(B2CClaimTypesConstants.ClaimTypeAction, B2CClaimTypesConstants.SignInFirstTimeAction)
            };
            var claimsIdentity = new ClaimsIdentity("custom_auth");
            claimsIdentity.AddClaims(claims);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(claimsIdentity);
            _httpContext = new DefaultHttpContext
            {
                Session = Session,
                User = claimsPrincipal
            };
            _systemUnderTest.PageContext = CreatePageContext(_httpContext);
            _mockBusinessAccountService.Setup(m => m.GetExternalUserAccountByIdAsync(It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(_externalUserAccount));
            var updatedAccounts = new List<ExternalUserAccount>() { _externalUserAccount };
            _mockBusinessAccountService.Setup(m => m.UpdateExternalUserAccountsAsync(It.IsAny<List<ExternalUserAccount>>(), It.IsAny<Guid>())).Returns(Task.FromResult(updatedAccounts));
            _mockBusinessAccountService.Setup(m => m.GetInvitesForUserAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_invites));
            _mockBusinessAccountService.Setup(m => m.GetAllInviteStatusAsync()).Returns(Task.FromResult(_inviteStatuses));
            _mockBusinessAccountService.Setup(m => m.UpdateInviteAsync(It.IsAny<Invite>(), It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Invite>()));
            _mockConfig.Setup(m => m["AzureAdB2C:TermsConditionsVersion"]).Returns("20221005");

            // Act
            var result = await _systemUnderTest.OnGet();

            // Assert
            using (new AssertionScope())
            {
                Assert.IsNotNull(result);
                result.Should().BeOfType<RedirectToPageResult>();
                if (result is RedirectToPageResult result1)
                {
                    result1.PageName.Should().Be(@Routes.Pages.Path.CD155a);
                }
            };
        }

        [Test]
        public async Task OnGet_Returns_Redirect_When_InValidClaimsProvided()
        {
            // Arrange
            _mockBusinessAccountService.Setup(m => m.GetExternalUserAccountByIdAsync(It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(_externalUserAccount));
            var updatedAccounts = new List<ExternalUserAccount>() { _externalUserAccount };
            _mockBusinessAccountService.Setup(m => m.UpdateExternalUserAccountsAsync(It.IsAny<List<ExternalUserAccount>>(), It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(updatedAccounts));

            // Act
            var result = await _systemUnderTest.OnGet();

            // Assert
            result.Should().BeOfType<RedirectToPageResult>();
            if (result is RedirectToPageResult res)
            {
                res.PageName.Should().Be("/Shared/InternalError");
            }
        }

        [Test]
        public void OnPostHomeButton_Returns_Redirect_When_ButtonClicked()
        {
            // Arrange & Act
            var result = _systemUnderTest.OnPostHomeButton();

            // Assert
            result.Should().BeOfType<RedirectResult>();
        }
    }
}
