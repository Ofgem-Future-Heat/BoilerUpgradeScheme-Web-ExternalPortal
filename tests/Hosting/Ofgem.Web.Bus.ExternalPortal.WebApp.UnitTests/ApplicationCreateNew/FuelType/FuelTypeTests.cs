using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.FuelType;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class FuelTypeTests : PageModelTestsBase
{
    private FuelTypeModel _systemUnderTest;
    protected Mock<ILogger<FuelTypeModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new FuelTypeModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new FuelTypeModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            PreviousFuelType = "Methane"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act.
        _ = _systemUnderTest.OnGet();

        // Assert
        var session = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            session!.PreviousFuelType.Should().Be(sessionModel.PreviousFuelType);
            _systemUnderTest.FuelType.Should().Be(sessionModel.PreviousFuelType);
        }
    }

    [Test]
    public void OnGet_Should_Leave_Empty_Form_Values_When_No_Model_In_Session()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;
        Session.Clear();

        // Act.
        _ = _systemUnderTest.OnGet();

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.FuelType.Should().BeNullOrEmpty();
            _systemUnderTest.FuelTypeOther.Should().BeNullOrEmpty();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_PropertyOwner_Name()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            PreviousFuelType = "Oil"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;
        _systemUnderTest.FuelType = "Oil";

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionModel.Should().NotBeNull();
            sessionModel?.PreviousFuelType.Should().Be(_systemUnderTest.FuelType);
        }
    }

    [Test]
    public void OnPost_Should_Errors_From_Incomplete_User_Response()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        _systemUnderTest.FuelType = "Other";
        _systemUnderTest.FuelTypeOther = String.Empty;

        _systemUnderTest.ModelState.AddModelError(nameof(FuelTypeModel), "What is the primary fuel type that the new heating system will replace?");

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        var sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionModel.Should().BeNull();
        }
    }

    [Test]
    public void OnPost_Should_Errors_From_Lack_Of_User_Response()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;
        _systemUnderTest.FuelType = "Oil";
        _systemUnderTest.ModelState.AddModelError(nameof(FuelTypeModel), "What is the primary fuel type that the new heating system will replace?");

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        var sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionModel.Should().BeNull();
        }
    }
}
