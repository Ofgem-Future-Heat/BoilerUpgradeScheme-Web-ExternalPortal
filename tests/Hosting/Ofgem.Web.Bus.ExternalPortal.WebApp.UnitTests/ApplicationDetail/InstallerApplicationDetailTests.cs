using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.Applications.Domain.Constants;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ofgem.API.BUS.Applications.Domain.ApplicationSubStatus;
using static Ofgem.API.BUS.Applications.Domain.VoucherSubStatus;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.ApplicationDetail;

[TestFixture]
public class InstallerApplicationDetailTests : ApplicationPageTestsBase<InstallerApplicationDetailModel>
{
    private static Guid ApplicationId = Guid.NewGuid();
    private static Guid BusinessAccountId = Guid.Parse("e8a1efcd-8ef7-4dd3-bac1-aedc38c67221");
    private static Guid SubmitterId = Guid.NewGuid();

    private InstallerApplicationDetailModel GenerateSystemUnderTest() =>
        new(_mockLogger.Object, _mockBusinessAccountService.Object, _mockApplicationsService.Object, _sessionService);

    private Application GenerateApplication() =>
        new()
        {
            ID = ApplicationId,
            CreatedBy = "User",
            CreatedDate = DateTime.Now.AddDays(-1),
            ApplicationDate = DateTime.Now.AddDays(-1),
            SubStatusId = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB],
            SubStatus = new ApplicationSubStatus { Id = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB] },
            ReferenceNumber = "GID12345",
            Voucher = new()
            {
                VoucherSubStatusID = StatusMappings.VoucherSubStatus[VoucherSubStatusCode.SUB],
                VoucherSubStatus = new VoucherSubStatus { Id = StatusMappings.VoucherSubStatus[VoucherSubStatusCode.SUB] }
            },
            InstallationAddress = new(),
            SubmitterId = SubmitterId,
            BusinessAccountId = BusinessAccountId,
            ConsentRequests = new List<ConsentRequest>
            {
                new()
                {
                    ConsentIssuedDate = DateTime.Now.AddDays(-1),
                    ConsentExpiryDate = DateTime.Now.AddDays(10),
                    ConsentReceivedDate = DateTime.Now,
                    CreatedDate = DateTime.Now.AddDays(-1)
                },
            }
        };

    private BusinessAccount GenerateBusinessAccount() =>
        new()
        {
            Id = BusinessAccountId
        };

    private List<ExternalUserAccount> GenerateUserAccounts() =>
        new List<ExternalUserAccount>
        {
            new ExternalUserAccount
            {
                Id = SubmitterId,
                BusinessAccountID = BusinessAccountId,
                FullName = "Tester Tester",
                EmailAddress = "test@test.com"
            }
        };

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationDetailModel(null!, _mockBusinessAccountService.Object, _mockApplicationsService.Object, _sessionService);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_ApplicationsService_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationDetailModel(_mockLogger.Object, _mockBusinessAccountService.Object, null!, _sessionService);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("applicationsService");
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_BusinessAccountsService_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationDetailModel(_mockLogger.Object, null!, _mockApplicationsService.Object, _sessionService);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("businessAccountsService");
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_SessionService_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationDetailModel(_mockLogger.Object, _mockBusinessAccountService.Object, _mockApplicationsService.Object, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("session");
    }

    [Test]
    public void Constructor_Should_Instantiate_With_Valid_Parameters()
    {
        // Arrange and act.
        var systemUnderTest = GenerateSystemUnderTest();

        // Assert
        systemUnderTest.Should().NotBeNull();
    }

    [Test]
    public async Task OnGetAsync_Should_Load_Application_Data()
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        var getResult = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        using (new AssertionScope())
        {
            systemUnderTest.Application.Should().Be(application);
            systemUnderTest.ConsentRequest.Should().Be(application.ConsentRequests.First());
            systemUnderTest.CurrentStatusId.Should().Be(application.Voucher?.VoucherSubStatusID ?? application.SubStatusId ?? StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB]);
            systemUnderTest.BusinessAccount.Should().Be(businessAccount);
            systemUnderTest.ApplicationSubmitter.Should().Be(businessAccountUsers.Single());

            getResult.Should().BeAssignableTo<PageResult>();
        }
    }

    [Test]
    public async Task OnGetAsync_Should_Redirect_To_Error_If_No_ReferenceNumber()
    {
        // Arrange
        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        var getResult = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        using (new AssertionScope())
        {
            getResult.Should().BeOfType<RedirectToPageResult>();
            ((RedirectToPageResult)getResult).PageName.Should().Be("/Shared/InternalError");
        }
    }

    [Test]
    public async Task OnGetAsync_Should_Redirect_To_Error_If_Application_Not_Found()
    {
        // Arrange
        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(null as Application);
        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        var getResult = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        using (new AssertionScope())
        {
            getResult.Should().BeOfType<RedirectToPageResult>();
            ((RedirectToPageResult)getResult).PageName.Should().Be("/Shared/InternalError");
        }
    }

    [Test]
    public async Task OnGetAsync_Should_Redirect_To_Error_If_BusinessAccount_Not_Found()
    {
        // Arrange
        var application = GenerateApplication();

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns((BusinessAccount)null!);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        var getResult = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        using (new AssertionScope())
        {
            getResult.Should().BeOfType<RedirectToPageResult>();
            ((RedirectToPageResult)getResult).PageName.Should().Be("/Shared/InternalError");
        }
    }

    [Test]
    public async Task OnGetAsync_Should_Redirect_To_Error_Submitter_Not_Found()
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(Enumerable.Empty<ExternalUserAccount>());

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        var getResult = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        using (new AssertionScope())
        {
            getResult.Should().BeOfType<RedirectToPageResult>();
            ((RedirectToPageResult)getResult).PageName.Should().Be("/Shared/InternalError");
        }
    }

    [Test]
    public async Task OnGetAsync_Should_Populate_CurrentContact_With_Submitter_Details_By_Default()
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        var expectedUser = businessAccountUsers.Single(s => s.Id == application.SubmitterId);

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        using (new AssertionScope())
        {
            systemUnderTest.CurrentContactEmailAddress.Should().Be(expectedUser.EmailAddress);
            systemUnderTest.CurrentContactFullName.Should().Be(expectedUser.FullName);
        }
    }

    [Test]
    public async Task OnGetAsync_Should_Populate_CurrentContact_With_Current_Contact_Details_When_Set()
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        var currentContactId = Guid.NewGuid();
        application.CurrentContactId = currentContactId;
        businessAccountUsers.Add(new ExternalUserAccount { Id = currentContactId, FullName = "Current Contact Name", EmailAddress = "current@contact.com" });

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        var expectedUser = businessAccountUsers.Single(s => s.Id == application.CurrentContactId);

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        using (new AssertionScope())
        {
            systemUnderTest.CurrentContactEmailAddress.Should().Be(expectedUser.EmailAddress);
            systemUnderTest.CurrentContactFullName.Should().Be(expectedUser.FullName);
        }
    }

    static object[] ConsentStateTestCaseSource =
    {
        new object[] { ApplicationSubStatusCode.INRW, new DateTime(2022, 1, 1), true },
        new object[] { ApplicationSubStatusCode.INRW, null!, false },
        new object[] { ApplicationSubStatusCode.WITH, new DateTime(2022, 1, 1), true },
        new object[] { ApplicationSubStatusCode.WITH, null!, false },
        new object[] { ApplicationSubStatusCode.CNTRW, new DateTime(2022, 1, 1), true },
        new object[] { ApplicationSubStatusCode.CNTRW, null!, false }
    };

    [TestCaseSource(nameof(ConsentStateTestCaseSource))]
    public async Task OnGetAsync_Should_Show_Or_Hide_ConsentState_Depending_On_Status(ApplicationSubStatusCode applicationSubStatus, DateTime? consentReceivedDate, bool expectedResult)
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        application.SubStatusId = StatusMappings.ApplicationSubStatus[applicationSubStatus];
        application.Voucher!.VoucherSubStatusID = null;
        application.ConsentRequests = new List<ConsentRequest>
        {
            new ConsentRequest {ConsentReceivedDate =consentReceivedDate, CreatedDate = DateTime.Now }
        };

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        systemUnderTest.ShowConsentReceived.Should().Be(expectedResult);
    }

    [TestCase(VoucherSubStatusCode.SUB, true)]
    [TestCase(VoucherSubStatusCode.REDREV, true)]
    [TestCase(VoucherSubStatusCode.WITHIN, true)]
    [TestCase(VoucherSubStatusCode.QC, true)]
    [TestCase(VoucherSubStatusCode.DA, true)]
    [TestCase(VoucherSubStatusCode.REDAPP, true)]
    [TestCase(VoucherSubStatusCode.SENTPAY, true)]
    [TestCase(VoucherSubStatusCode.PAID, true)]
    [TestCase(VoucherSubStatusCode.REJPEND, true)]
    [TestCase(VoucherSubStatusCode.REJECTED, true)]
    [TestCase(VoucherSubStatusCode.PAYSUS, true)]
    [TestCase(VoucherSubStatusCode.REVOKED, true)]
    [TestCase(null!, false)]
    public async Task OnGetAsync_Should_Set_VoucherRedemptionState_When_Voucher_Status_Set(VoucherSubStatusCode? voucherSubStatus, bool expectedResult)
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        application.Voucher!.VoucherSubStatusID = voucherSubStatus != null ? StatusMappings.VoucherSubStatus[voucherSubStatus.Value] : null;

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        systemUnderTest.IsVoucherRedeemed.Should().Be(expectedResult);
    }

    [TestCase(ApplicationSubStatusCode.SUB, true)]
    [TestCase(ApplicationSubStatusCode.INRW, true)]
    [TestCase(ApplicationSubStatusCode.WITH, true)]
    [TestCase(ApplicationSubStatusCode.QC, true)]
    [TestCase(ApplicationSubStatusCode.DA, true)]
    [TestCase(ApplicationSubStatusCode.VPEND, true)]
    [TestCase(ApplicationSubStatusCode.VQUED, true)]
    [TestCase(ApplicationSubStatusCode.CNTRW, true)]
    [TestCase(ApplicationSubStatusCode.CNTRD, true)]
    [TestCase(ApplicationSubStatusCode.RPEND, false)]
    [TestCase(ApplicationSubStatusCode.REJECTED, false)]
    [TestCase(ApplicationSubStatusCode.WITHDRAWN, false)]
    public async Task OnGetAsync_Should_Enable_CancelApplicationButton_When_In_Correct_Status(ApplicationSubStatusCode applicationSubStatus, bool expectedResult)
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        application.SubStatusId = StatusMappings.ApplicationSubStatus[applicationSubStatus];
        application.Voucher!.VoucherSubStatusID = null;

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        systemUnderTest.ShowCancelApplicationButton.Should().Be(expectedResult);
    }

    [TestCase(ApplicationSubStatusCode.WITH, false)]
    [TestCase(ApplicationSubStatusCode.DA, false)]
    [TestCase(ApplicationSubStatusCode.SUB, false)]
    [TestCase(ApplicationSubStatusCode.REJECTED, false)]
    [TestCase(ApplicationSubStatusCode.VISSD, true)]
    public async Task OnGetAsync_Should_Enable_RedeemVoucherButton_When_In_Correct_Status(ApplicationSubStatusCode applicationSubStatus, bool expectedResult)
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        application.SubStatusId = StatusMappings.ApplicationSubStatus[applicationSubStatus];
        application.Voucher!.VoucherSubStatusID = null;

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        systemUnderTest.ShowRedeemVoucherButton.Should().Be(expectedResult);
    }

    [TestCase(ApplicationSubStatusCode.WITH, false)]
    [TestCase(ApplicationSubStatusCode.DA, false)]
    [TestCase(ApplicationSubStatusCode.SUB, false)]
    [TestCase(ApplicationSubStatusCode.REJECTED, false)]
    [TestCase(ApplicationSubStatusCode.VISSD, true)]
    public async Task OnGetAsync_Should_Enable_CancelVoucherButton_When_In_Correct_Status(ApplicationSubStatusCode applicationSubStatus, bool expectedResult)
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        application.SubStatusId = StatusMappings.ApplicationSubStatus[applicationSubStatus];
        application.Voucher!.VoucherSubStatusID = null;

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        systemUnderTest.ShowCancelVoucherButton.Should().Be(expectedResult);
    }

    [TestCase(ApplicationSubStatusCode.SUB, null, true)]
    [TestCase(ApplicationSubStatusCode.INRW, null, true)]
    [TestCase(ApplicationSubStatusCode.WITH, null, true)]
    [TestCase(ApplicationSubStatusCode.QC, null, true)]
    [TestCase(ApplicationSubStatusCode.DA, null, true)]
    [TestCase(ApplicationSubStatusCode.VPEND, null, true)]
    [TestCase(ApplicationSubStatusCode.VQUED, null, true)]
    [TestCase(ApplicationSubStatusCode.VEXPD, null, true)]
    [TestCase(ApplicationSubStatusCode.CNTRW, null, true)]
    [TestCase(ApplicationSubStatusCode.CNTPS, null, true)]
    [TestCase(ApplicationSubStatusCode.CNTRD, null, true)]
    [TestCase(ApplicationSubStatusCode.RPEND, null, true)]
    [TestCase(ApplicationSubStatusCode.REJECTED, null, true)]
    [TestCase(ApplicationSubStatusCode.WITHDRAWN, null, true)]
    [TestCase(ApplicationSubStatusCode.VISSD, VoucherSubStatusCode.REVOKED, true)]
    [TestCase(ApplicationSubStatusCode.VISSD, VoucherSubStatusCode.SUB, false)]
    [TestCase(ApplicationSubStatusCode.VISSD, VoucherSubStatusCode.REDREV, false)]
    [TestCase(ApplicationSubStatusCode.VISSD, VoucherSubStatusCode.WITHIN, false)]
    public async Task OnGetAsync_Should_Show_Application_Details_When_In_Correct_Status(ApplicationSubStatusCode applicationSubStatus, VoucherSubStatusCode? voucherSubStatus, bool expectedResult)
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        application.SubStatusId = StatusMappings.ApplicationSubStatus[applicationSubStatus];
        application.Voucher!.VoucherSubStatusID = voucherSubStatus != null ? StatusMappings.VoucherSubStatus[voucherSubStatus.Value] : null;

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        systemUnderTest.ShowApplicationDetails.Should().Be(expectedResult);
    }

    [TestCase(ApplicationSubStatusCode.WITH, false)]
    [TestCase(ApplicationSubStatusCode.DA, false)]
    [TestCase(ApplicationSubStatusCode.SUB, false)]
    [TestCase(ApplicationSubStatusCode.REJECTED, false)]
    [TestCase(ApplicationSubStatusCode.VISSD, true)]
    public async Task OnGetAsync_Should_Show_Voucher_Details_When_In_Correct_Status(ApplicationSubStatusCode applicationSubStatus, bool expectedResult)
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        application.SubStatusId = StatusMappings.ApplicationSubStatus[applicationSubStatus];
        application.Voucher!.VoucherSubStatusID = null;

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        systemUnderTest.ShowVoucherDetails.Should().Be(expectedResult);
    }

    [TestCase(VoucherSubStatusCode.SUB, true)]
    [TestCase(VoucherSubStatusCode.REDREV, true)]
    [TestCase(VoucherSubStatusCode.WITHIN, true)]
    [TestCase(VoucherSubStatusCode.QC, true)]
    [TestCase(VoucherSubStatusCode.DA, true)]
    [TestCase(VoucherSubStatusCode.REDAPP, true)]
    [TestCase(VoucherSubStatusCode.SENTPAY, true)]
    [TestCase(VoucherSubStatusCode.PAID, true)]
    [TestCase(VoucherSubStatusCode.REJPEND, true)]
    [TestCase(VoucherSubStatusCode.REJECTED, true)]
    [TestCase(VoucherSubStatusCode.PAYSUS, true)]
    [TestCase(VoucherSubStatusCode.REVOKED, false)]
    public async Task OnGetAsync_Should_Show_Redemption_Details_When_In_Correct_Status(VoucherSubStatusCode voucherSubStatus, bool expectedResult)
    {
        // Arrange
        var application = GenerateApplication();
        var businessAccount = GenerateBusinessAccount();
        var businessAccountUsers = GenerateUserAccounts();

        application.SubStatusId = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.VISSD];
        application.Voucher!.VoucherSubStatusID = StatusMappings.VoucherSubStatus[voucherSubStatus];

        _mockApplicationsService.Setup(m => m.GetApplicationByReferenceNumberAsync(It.IsAny<string>()).Result).Returns(application);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountAsync(It.IsAny<Guid>()).Result).Returns(businessAccount);
        _mockBusinessAccountService.Setup(m => m.ExternalGetBusinessAccountUsersAsync(It.IsAny<Guid>()).Result).Returns(businessAccountUsers);

        var systemUnderTest = GenerateSystemUnderTest();

        // Act
        _ = await systemUnderTest.OnGetAsync("GID1234");

        // Assert
        systemUnderTest.ShowRedemptionDetails.Should().Be(expectedResult);
    }
}