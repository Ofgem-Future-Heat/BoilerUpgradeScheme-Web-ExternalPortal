using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Domain.Entities.Views;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.API.BUS.BusinessAccounts.Domain.Exceptions;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationsDashboard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Ofgem.API.BUS.BusinessAccounts.Domain.Entities.BusinessAccountSubStatus;

namespace Ofgem.Web.BUS.InternalPortal.WebApp.UnitTests.Pages.Dashboard;

[TestFixture]
public class InstallerApplicationDetailTests : ApplicationPageTestsBase<InstallerApplicationsModel>
{
    private InstallerApplicationsModel _systemUnderTest = null!;
    private readonly Mock<IConfiguration> _mockConfiguration = new();

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationsModel(null!, _mockBusinessAccountService.Object, _mockApplicationsService.Object, _sessionService, _mockConfiguration.Object);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_ApplicationsService_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationsModel(_mockLogger.Object, _mockBusinessAccountService.Object, null!, _sessionService, _mockConfiguration.Object);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("applicationsService");
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_BusinessAccountsService_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationsModel(_mockLogger.Object, null!, _mockApplicationsService.Object, _sessionService, _mockConfiguration.Object);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("businessAccountsService");
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_SessionService_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationsModel(_mockLogger.Object, _mockBusinessAccountService.Object, _mockApplicationsService.Object, null!, _mockConfiguration.Object);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("session");
    }

    private InstallerApplicationsModel GenerateSystemUnderTest() => new(_mockLogger.Object, _mockBusinessAccountService.Object, _mockApplicationsService.Object, _sessionService, _mockConfiguration.Object);

    [Test]
    public void Constructor_Should_Instantiate_With_Valid_Parameters()
    {
        // Arrange and act.
        _systemUnderTest = GenerateSystemUnderTest();

        // Assert
        _systemUnderTest.Should().NotBeNull();
    }

    [Test]
    public async Task OnGetAsync_No_Business_Account_Got_Error_Page()
    {
        // Arrange
        _systemUnderTest = GenerateSystemUnderTest();

        _mockBusinessAccountService.Setup(d => d.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns((BusinessAccount)null!);

        // Act.
        await _systemUnderTest.OnGetAsync("", "", "", true);

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.BusinessAccountData.Should().BeNull();
            _systemUnderTest.Applications.Should().BeNullOrEmpty();
        }
    }

    [Test]
    public async Task OnGetAsync_Not_A_Valid_Business_Account_Throws_Error()
    {
        // Arrange
        _systemUnderTest = GenerateSystemUnderTest();

        _mockBusinessAccountService.Setup(d => d.ExternalGetBusinessAccountAsync(It.IsAny<Guid>())).Throws(new ResourceNotFoundException(""));

        // Act.
        Func<Task> act = () => _ = _systemUnderTest.OnGetAsync("", "", "", true);

        // Assert
        await act.Should().ThrowExactlyAsync<ResourceNotFoundException>();
    }

    [Test]
    public async Task OnGetAsync_No_Applications_Goto_No_Applications_Page()
    {
        // Arrange
        _systemUnderTest = GenerateSystemUnderTest();

        _mockBusinessAccountService.Setup(d => d.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()))
                                   .ReturnsAsync(new BusinessAccount() { Id = new Guid("FD5D5B0E-9F92-47E2-6D03-08DA38AD8E59") });

        _mockApplicationsService.Setup(d => d.GetDashboardApplicationsByBusinessAccountIdAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>()))
                                .ReturnsAsync(new List<ExternalPortalDashboardApplication>());

        // Act.
        await _systemUnderTest.OnGetAsync("", "", "", true);

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.BusinessAccountData.Should().NotBeNull();
            _systemUnderTest.Applications.Should().BeEmpty();
        }
    }

    [Test]
    public async Task OnPostAsync_Apply_Search_To_Applications_Data_With_No_Create_Button()
    {
        // Arrange
        _systemUnderTest = GenerateSystemUnderTest();

        _mockBusinessAccountService.Setup(d => d.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()))
                                   .ReturnsAsync(new BusinessAccount() { Id = new Guid("FD5D5B0E-9F92-47E2-6D03-08DA38AD8E59") });
        _mockApplicationsService.Setup(d => d.GetDashboardApplicationsByBusinessAccountIdAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>()))
                                .ReturnsAsync(new List<ExternalPortalDashboardApplication>());

        // Act - Business Account Active date = null.
        await _systemUnderTest.OnGetAsync("", "", "", true);

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.BusinessAccountData.Should().NotBeNull();
            _systemUnderTest.Applications.Should().BeEmpty();
        }
    }

    [Test]
    public async Task OnPostAsync_Clear_Search_And_Filter_Selections()
    {
        // Arrange
        _systemUnderTest = GenerateSystemUnderTest();

        var applicationId = new Guid("0C825E3F-EB21-46A6-2870-08DA2C22AA69");

        _mockBusinessAccountService.Setup(d => d.ExternalGetBusinessAccountAsync(applicationId))
                                   .ReturnsAsync(new BusinessAccount() { Id = new Guid("FD5D5B0E-9F92-47E2-6D03-08DA38AD8E59") });

        _mockApplicationsService.Setup(d => d.GetDashboardApplicationsByBusinessAccountIdAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>()))
                                .ReturnsAsync(new List<ExternalPortalDashboardApplication>());
        _systemUnderTest.SearchBy = "";
        _sessionService.BusinessAccountId = applicationId;

        // Act.
        await _systemUnderTest.OnGetAsync("", "", "", true);

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.BusinessAccountData.Should().NotBeNull();
            _systemUnderTest.Applications.Should().NotBeNull();
        }
    }

    [TestCase(BusinessAccountSubStatusCode.ACTIV, true, true)]
    [TestCase(BusinessAccountSubStatusCode.DA, true, true)]
    [TestCase(BusinessAccountSubStatusCode.INREV, true, true)]
    [TestCase(BusinessAccountSubStatusCode.QC, true, true)]
    [TestCase(BusinessAccountSubStatusCode.SUBMIT, true, true)]
    [TestCase(BusinessAccountSubStatusCode.SUSPEND, true, true)]
    [TestCase(BusinessAccountSubStatusCode.SUBMIT, true, true)]
    [TestCase(BusinessAccountSubStatusCode.WITHI, true, true)]
    [TestCase(BusinessAccountSubStatusCode.REVOK, true, false)]
    [TestCase(BusinessAccountSubStatusCode.FAIL, true, false)]
    [TestCase(BusinessAccountSubStatusCode.WITHDR, true, false)]
    [TestCase(BusinessAccountSubStatusCode.ACTIV, false, false)]
    [TestCase(BusinessAccountSubStatusCode.DA, false, false)]
    [TestCase(BusinessAccountSubStatusCode.INREV, false, false)]
    [TestCase(BusinessAccountSubStatusCode.QC, false, false)]
    [TestCase(BusinessAccountSubStatusCode.SUBMIT, false, false)]
    [TestCase(BusinessAccountSubStatusCode.SUSPEND, false, false)]
    [TestCase(BusinessAccountSubStatusCode.SUBMIT, false, false)]
    [TestCase(BusinessAccountSubStatusCode.WITHI, false, false)]
    [TestCase(BusinessAccountSubStatusCode.REVOK, false, false)]
    [TestCase(BusinessAccountSubStatusCode.FAIL, false, false)]
    [TestCase(BusinessAccountSubStatusCode.WITHDR, false, false)]
    public async Task OnGetAsync_Create_Button_Exists(BusinessAccountSubStatusCode statusCode, bool hasActiveDate, bool assertion)
    {
        // Arrange
        DateTime? activeDate = null;
        if (hasActiveDate)
            activeDate = DateTime.UtcNow;

        _mockBusinessAccountService.Setup(d => d.ExternalGetBusinessAccountAsync(Guid.Parse("e8a1efcd-8ef7-4dd3-bac1-aedc38c67221")))
            .ReturnsAsync(
                new BusinessAccount
                {
                    Id = Guid.Parse("e8a1efcd-8ef7-4dd3-bac1-aedc38c67221"),
                    ActiveDate = activeDate,
                    SubStatus = new BusinessAccountSubStatus
                    {
                        Code = statusCode
                    }
                });
        
        _mockApplicationsService.Setup(d => d.GetDashboardApplicationsByBusinessAccountIdAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ExternalPortalDashboardApplication>());

        _systemUnderTest = GenerateSystemUnderTest();

        // Act - Business Account Active date = null.
        await _systemUnderTest.OnGetAsync("", "", "", true);

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.IsAllowedCreateNewCase.Should().Be(assertion);
        }
    }
}