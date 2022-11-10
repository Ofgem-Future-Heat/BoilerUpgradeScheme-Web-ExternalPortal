using FluentAssertions;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.ApplicationDetail;

[TestFixture]
public class InstallerApplicationNewStartTests : ApplicationPageTestsBase<InstallerApplicationNewStartModel>
{
    private readonly InstallerApplicationNewStartModel _systemUnderTest = null!;

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationNewStartModel(null, _sessionService);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Session_Null()
    {
        // Arrange and act.
        var action = () => new InstallerApplicationNewStartModel(_mockLogger.Object, null);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("session");
    }
}