using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.DropOut;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.TechType;
using System;
using System.Collections.Generic;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class DropOutTests : PageModelTestsBase
{
    private DropOutModel _systemUnderTest;
    protected Mock<ILogger<DropOutModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new DropOutModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new BiomassSpecModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnPost_Should_Process_DropOut_Message_No_Key_Found_Error()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act.
        var action = () => _systemUnderTest.OnGet("");

        // Assert
        action.Should().Throw<KeyNotFoundException>();
    }

    [Test]
    [TestCase("CD05", "You cannot apply for a grant if the installation address is outside of England and Wales.")]
    [TestCase("CD12", "You have told us your project does not meet the scheme's eligibility criteria.")]
    public void OnPost_Should_Process_DropOut_Message(string key, string message)
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act.
        _ = _systemUnderTest.OnGet(key);

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.ErrorMessage.Should().Be(message);
        }
    }
}
