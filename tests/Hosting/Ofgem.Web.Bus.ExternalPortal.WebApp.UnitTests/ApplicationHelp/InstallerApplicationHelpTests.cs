using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationHelp;
using System;
using System.Threading.Tasks;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.ApplicationHelp;

[TestFixture]
public class InstallerApplicationHelpTests : ApplicationPageTestsBase<InstallerHelpModel>
{
    private InstallerHelpModel _systemUnderTest = null!;

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and act.
        var action = () => new InstallerHelpModel(_sessionService, _mockBusinessAccountService.Object, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    private InstallerHelpModel GenerateSystemUnderTest() => new(_sessionService, _mockBusinessAccountService.Object, _mockLogger.Object);

    [Test]
    public void Constructor_Should_Instantiate_With_Valid_Parameters()
    {
        // Arrange and act.
        _systemUnderTest = GenerateSystemUnderTest();

        // Assert
        _systemUnderTest.Should().NotBeNull();
    }

    [Test]
    public async Task OnGet_No_ReferenceNumber_In_Session_Go_Error_Page()
    {
        // Arrange
        _sessionService.BusinessAccountId = Guid.Empty;
        _systemUnderTest = GenerateSystemUnderTest();

        // Act.
        await _systemUnderTest.OnGetAsync();

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.BusinessAccountId.Should().Be(Guid.Empty);
        }
    }

    [Test]
    public async Task OnGet_Page_Return()
    {
        // Arrange
        var businessAccountId = Guid.NewGuid();
        var businessAccountNumber = "BUS-123";
        var referenceNumber = "Ref-123";

        var businessData = new BusinessAccount
        {
            BusinessAccountNumber = businessAccountNumber
        };

        _sessionService.BusinessAccountId = businessAccountId;
        _sessionService.ReferenceNumber = referenceNumber;

        _mockBusinessAccountService.Setup(x => x.ExternalGetBusinessAccountAsync(businessAccountId)).Returns(Task.FromResult(businessData));

        _systemUnderTest = GenerateSystemUnderTest();


        // Act.
        await _systemUnderTest.OnGetAsync();

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.BusinessAccountNumber.Should().Be(businessAccountNumber);
            _systemUnderTest.BusinessAccountId.Should().Be(businessAccountId);
        }
    }
}
