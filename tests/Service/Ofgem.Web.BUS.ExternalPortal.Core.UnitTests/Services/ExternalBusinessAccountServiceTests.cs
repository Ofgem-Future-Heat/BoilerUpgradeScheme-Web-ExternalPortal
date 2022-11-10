using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.BusinessAccounts.Client.Interfaces;
using Ofgem.API.BUS.BusinessAccounts.Domain.Request;
using System;
using System.Threading.Tasks;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using System.Security.Claims;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using System.Collections.Generic;
using Ofgem.Lib.BUS.APIClient.Domain.Models;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Services;

public class ExternalBusinessAccountServiceTests
{
    private ExternalBusinessAccountService? _systemUnderTest;

    private Mock<IExternalBusinessAccountsAPIClient> _mockBusinessAccountsAPIClient = new();
    private Mock<IExternalBusinessAccountRequestsClient> _mockAccountRequestsClient = new();
    private Mock<ClaimsPrincipal> _mockClaimsPrincipal = new();

    private readonly ExternalUserAccount externalUserAccount = new ExternalUserAccount
    {
        Id = Guid.NewGuid(),
        AuthorisedRepresentative = true,
        SuperUser = true,
        EmailAddress = "TestSuperUser@ofgem.gov.uk",
        TelephoneNumber = "01234567890",
        CreatedDate = new DateTime(2022, 08, 13),
        StandardUser = false
    };

    [OneTimeSetUp]
    public void FixtureSetup()
    {
        _mockBusinessAccountsAPIClient = new Mock<IExternalBusinessAccountsAPIClient>();
        _mockAccountRequestsClient = new Mock<IExternalBusinessAccountRequestsClient>();

        _mockBusinessAccountsAPIClient.Setup(x => x.ExternalBusinessAccountRequestsClient).Returns(_mockAccountRequestsClient.Object);
    }

    [Test]
    public void Constructor_Throws_ArgumentNullException_If_IBusinessAccountsAPIClient_Is_Null()
    {
        // Arrange and Act.
        IExternalBusinessAccountsAPIClient? externalBusinessAccountsApiClient = null!;
        Func<ExternalBusinessAccountService>? action = () => new ExternalBusinessAccountService(externalBusinessAccountsApiClient, _mockClaimsPrincipal.Object);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("externalBusinessAccountsApiClient");
    }

    [Test]
    public void Can_Be_Instantiated_With_Valid_Parameters()
    {
        // Arrange & Act.
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);

        // Assert
        _systemUnderTest.Should().NotBeNull();
    }

    [Test]
    public async Task ExternalGetBusinessAccountAsync_Calls_Client_SuccessfullyAsync()
    {
        // Arrange
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);
        var businessAccountId = Guid.NewGuid();

        // Act.
        _ = await _systemUnderTest.ExternalGetBusinessAccountAsync(businessAccountId);

        // Assert
        _mockBusinessAccountsAPIClient.Verify(mock => mock.ExternalBusinessAccountRequestsClient.ExternalGetBusinessAccountById(businessAccountId), Times.Once);
    }

    [Test]
    public async Task ExternalGetBusinessAccountUsersAsync_Calls_Client_SuccessfullyAsync()
    {
        // Arrange
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);
        var businessAccountId = Guid.NewGuid();

        // Act.
        _ = await _systemUnderTest.ExternalGetBusinessAccountUsersAsync(businessAccountId);

        // Assert
        _mockBusinessAccountsAPIClient.Verify(mock => mock.ExternalBusinessAccountRequestsClient.ExternalGetBusinessAccountUsersAsync(businessAccountId), Times.Once);
    }

    [Test]
    public async Task ExternalGetBusinessAccountAsync_Calls_Client_WithNull_Parameter()
    {
        // Arrange
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);

        // Act.
        _ = await _systemUnderTest.ExternalGetBusinessAccountAsync(null);

        // Assert
        _mockBusinessAccountsAPIClient.Verify(mock => mock.ExternalBusinessAccountRequestsClient.ExternalGetBusinessAccountById(null), Times.Once);
    }

    [Test]
    public async Task GetExternalUserAccountByIdAsync_Calls_Client_Successfully()
    {
        // Arrange
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);

        // Act
        _ = await _systemUnderTest.GetExternalUserAccountByIdAsync(Guid.NewGuid());

        // Assert
        _mockBusinessAccountsAPIClient.Verify(mock => mock.ExternalBusinessAccountRequestsClient.GetExternalUserAccountById(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task UpdateExternalUserAccountsAsync_Calls_Client_Successfully()
    {
        // Arrange
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);
        var externalUsers = new List<ExternalUserAccount> { externalUserAccount };

        // Act
        _ = await _systemUnderTest.UpdateExternalUserAccountsAsync(externalUsers, Guid.NewGuid());

        // Assert
        _mockBusinessAccountsAPIClient.Verify(mock => mock.ExternalBusinessAccountRequestsClient.UpdateExternalUserAccountsAsync(It.IsAny<List<ExternalUserAccount>>(), It.IsAny<AuditLogParameters>()), Times.Once);
    }

    [Test]
    public async Task GetInvitesForUserAsync_Calls_Client_Successfully()
    {
        //Arrange
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);

        //Act
        await _systemUnderTest.GetInvitesForUserAsync(Guid.NewGuid());

        //Assert
        _mockBusinessAccountsAPIClient.Verify(mock => mock.ExternalBusinessAccountRequestsClient.GetInvitesForUserAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task UpdateInviteAsync_Calls_Client_Successfully()
    {
        //Arrange
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);

        //Act
        await _systemUnderTest.UpdateInviteAsync(new Invite(), Guid.NewGuid());

        //Assert
        _mockBusinessAccountsAPIClient.Verify(mock => mock.ExternalBusinessAccountRequestsClient.UpdateInviteAsync(It.IsAny<Invite>(), It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task GetAllInviteStatusAsync_Calls_Client_Successfully()
    {
        //Arrange
        _systemUnderTest = new ExternalBusinessAccountService(_mockBusinessAccountsAPIClient.Object, _mockClaimsPrincipal.Object);

        //Act
        await _systemUnderTest.GetAllInviteStatusAsync();

        //Assert
        _mockBusinessAccountsAPIClient.Verify(mock => mock.ExternalBusinessAccountRequestsClient.GetAllInviteStatusAsync(), Times.Once);
    }

}