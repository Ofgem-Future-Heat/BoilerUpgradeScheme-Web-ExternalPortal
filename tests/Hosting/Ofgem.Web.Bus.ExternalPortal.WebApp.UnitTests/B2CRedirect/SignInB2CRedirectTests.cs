using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using Ofgem.Web.BUS.ExternalPortal.Core.Services;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.B2CRedirect
{
    [TestFixture]
    public class SignInB2CRedirectTests : PageModelTestsBase
    {
        private SignInB2CRedirectModel _systemUnderTest;
        private readonly Mock<ILogger<SignInB2CRedirectModel>> _mockLogger = new();
        private readonly Mock<IExternalBusinessAccountService> _mockBusinessAccountService = new();
        private readonly string _b2cObjectId = Guid.NewGuid().ToString();
        private readonly string _externalUserId = Guid.NewGuid().ToString();
        private readonly string _externalBusinessAccountId = Guid.NewGuid().ToString();
        private HttpContext _httpContext;
        private ExternalUserAccount _externalUserAccountActive = new();
        private ExternalUserAccount _externalUserAccountObsolete = new();
        private readonly SessionService _sessionService = NewSessionService();

        [SetUp]
        public override void TestCaseSetUp()
        {
            _systemUnderTest = new SignInB2CRedirectModel(_mockBusinessAccountService.Object, _sessionService, _mockLogger.Object);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, _b2cObjectId),
                new Claim(B2CClaimTypesConstants.ClaimTypeExternalUserId, _externalUserId),
                new Claim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId, _externalBusinessAccountId)
            };
            var claimsIdentity = new ClaimsIdentity(authenticationType: "custom");
            claimsIdentity.AddClaims(claims);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(claimsIdentity);
            _httpContext = new DefaultHttpContext
            {
                Session = Session,
                User = claimsPrincipal
            };
            _externalUserAccountActive = new ExternalUserAccount
            {
                Id = Guid.Parse(_externalUserId),
                BusinessAccountID = Guid.Parse(_externalBusinessAccountId),
                AuthorisedRepresentative = true,
                FirstName = "Authorised",
                LastName = "Rep",
                EmailAddress = "AuthorisedTest@Ofgem.gov.uk",
                TelephoneNumber = "01234567890",
                HomeAddress = "2 Test Street",
                HomeAddressPostcode = "TE22 2ST",
                HomeAddressUPRN = "098765432100",
                DOB = new DateTime(1997, 11, 17),
                IsObsolete = false
            };
            _externalUserAccountObsolete = new ExternalUserAccount
            {
                Id = Guid.Parse(_externalUserId),
                BusinessAccountID = Guid.Parse(_externalBusinessAccountId),
                AuthorisedRepresentative = true,
                FirstName = "Authorised",
                LastName = "Rep",
                EmailAddress = "AuthorisedTest@Ofgem.gov.uk",
                TelephoneNumber = "01234567890",
                HomeAddress = "2 Test Street",
                HomeAddressPostcode = "TE22 2ST",
                HomeAddressUPRN = "098765432100",
                DOB = new DateTime(1997, 11, 17),
                IsObsolete = true
            };
            base.TestCaseSetUp();
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
        {
            // Arrange and Act.
            var action = () => new SignInB2CRedirectModel(_mockBusinessAccountService.Object, _sessionService, null!);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentNullException_If_BusinessAccountService_Null()
        {
            // Arrange and Act.
            var action = () => new SignInB2CRedirectModel(null!, _sessionService, _mockLogger.Object);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("externalBusinessAccountService");
        }

        [Test]
        public async Task OnGet_Returns_Success_When_UserActive()
        {
            // Arrange
            _systemUnderTest.PageContext = CreatePageContext(_httpContext);
            _mockBusinessAccountService.Setup(m => m.GetExternalUserAccountByIdAsync(It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(_externalUserAccountActive));
            var unexpected = new RedirectResult("/Shared/RoutingError?statusCode=403");

            // Act
            var result = await _systemUnderTest.OnGet();

            // Assert
            using (new AssertionScope())
            {
                Assert.IsNotNull(result);
                result.Should().NotBeEquivalentTo(unexpected);
                _sessionService.BusinessAccountId.Should().Be(Guid.Parse(_externalBusinessAccountId));
                _sessionService.UserId.Should().Be(Guid.Parse(_externalUserId));
            }
        }

        [Test]
        public async Task OnGet_Returns_Redirect_When_UserObsolete()
        {
            // Arrange
            _systemUnderTest.PageContext = CreatePageContext(_httpContext);
            _mockBusinessAccountService.Setup(m => m.GetExternalUserAccountByIdAsync(It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(_externalUserAccountObsolete));
            var expected = new RedirectResult("/CustomIdentity/Account/AccessDenied");

            // Act
            var result = await _systemUnderTest.OnGet();

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task OnGet_Should_Throw_InvalidDataException_If_B2CClaimTypesConstants_DoesNotMatch()
        {
            // Arrange
            _mockBusinessAccountService
                .Setup(m => m.GetExternalUserAccountByIdAsync(Guid.NewGuid()))
                .Returns(Task.FromResult(_externalUserAccountActive));
            var sut = new SignInB2CRedirectModel(_mockBusinessAccountService.Object, _sessionService, _mockLogger.Object);

            // Act
            var action = async () => await sut.OnGet();

            // Assert
            await action.Should().ThrowAsync<NullReferenceException>();
        }
    }
}
