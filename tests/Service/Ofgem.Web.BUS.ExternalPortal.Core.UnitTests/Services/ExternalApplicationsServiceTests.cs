using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Client.Interfaces;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Fakes;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Services;

[TestFixture]
public class ExternalApplicationsServiceTests
{
    private ExternalApplicationsService? _systemUnderTest;

    private Mock<IExternalApplicationsAPIClient> _mockApplicationsApiClient = new();
    private Mock<IExternalApplicationsRequestsClient> _mockApplicationsRequestsClient = new();
    private SessionService _sessionService = null!;

    private readonly Mock<IConfiguration> _mockConfiguration = new();

    [OneTimeSetUp]
    public void FixtureSetup()
    {
        // Configure requests clients
        _mockApplicationsApiClient = new Mock<IExternalApplicationsAPIClient>();
        _mockApplicationsRequestsClient = new Mock<IExternalApplicationsRequestsClient>();
        _mockApplicationsApiClient.Setup(x => x.ExternalApplicationsRequestsClient).Returns(_mockApplicationsRequestsClient.Object);

        // Configure session
        var httpContext = new DefaultHttpContext();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        mockHttpContextAccessor.Setup(x => x.HttpContext!.Session).Returns(new FakeHttpSession());

        _sessionService = new SessionService(mockHttpContextAccessor.Object, Mock.Of<ClaimsPrincipal>());
    }

    [Test]
    public void Constructor_Throws_ArgumentNullException_If_IApplicationsAPIClient_Is_Null()
    {
        // Arrange and Act.
        IExternalApplicationsAPIClient? externalApplicationsApiClient = null!;
        Func<ExternalApplicationsService>? action = () => new ExternalApplicationsService(externalApplicationsApiClient, _mockConfiguration.Object, _sessionService);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("externalApplicationsApiClient");
    }

    [Test]
    public void Can_Be_Instantiated_With_Valid_Parameters()
    {
        // Arrange & Act.
        _systemUnderTest = new ExternalApplicationsService(_mockApplicationsApiClient.Object, _mockConfiguration.Object, _sessionService);

        // Assert
        _systemUnderTest.Should().NotBeNull();
    }

    [Test]
    public async Task GetDashboardApplicationsByBusinessAccountIdAsync_Calls_Client_SuccessfullyAsync()
    {
        // Arrange
        _systemUnderTest = new ExternalApplicationsService(_mockApplicationsApiClient.Object , _mockConfiguration.Object, _sessionService);
        var businessAccountId = Guid.NewGuid();

        // Act.
        _ = await _systemUnderTest.GetDashboardApplicationsByBusinessAccountIdAsync(businessAccountId);

        // Assert
        _mockApplicationsApiClient.Verify(mock => mock.ExternalApplicationsRequestsClient.GetDashboardApplicationsByBusinessAccountIdAsync(businessAccountId, string.Empty, null, string.Empty), Times.Once);
    }

    [Test]
    public async Task GetApplicationByReferenceNumberAsync_Calls_Client_Successfuly()
    {
        // Arrange
        _systemUnderTest = new ExternalApplicationsService(_mockApplicationsApiClient.Object, _mockConfiguration.Object, _sessionService);
        var applicationReferenceNumber = "GID12345";

        // Act
        _ = await _systemUnderTest.GetApplicationByReferenceNumberAsync(applicationReferenceNumber);

        // Assert
        _mockApplicationsApiClient.Verify(mock => mock.ExternalApplicationsRequestsClient.GetApplicationByReferenceNumberAsync(applicationReferenceNumber), Times.Once);
    }
}
