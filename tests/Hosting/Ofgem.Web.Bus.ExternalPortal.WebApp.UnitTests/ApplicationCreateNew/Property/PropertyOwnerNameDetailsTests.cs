using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class PropertyOwnerNameDetailsTests : PageModelTestsBase
{
    private PropertyOwnerNameModel _systemUnderTest;
    protected Mock<ILogger<PropertyOwnerNameModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new PropertyOwnerNameModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new PropertyOwnerNameModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerName = "Test Test",
            PropertyOwnerTelephoneNumber = "01234567890"
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
        using (new AssertionScope())
        {
            _systemUnderTest.PropertyOwnerName.Should().Be(sessionModel.PropertyOwnerName);
            _systemUnderTest.PropertyOwnerTelephoneNumber.Should().Be(sessionModel.PropertyOwnerTelephoneNumber);
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
            _systemUnderTest.PropertyOwnerName.Should().BeNull();
            _systemUnderTest.PropertyOwnerTelephoneNumber.Should().BeNull();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_Property_Owner_Contact_Details()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerName = "Test Test",
            PropertyOwnerTelephoneNumber = "01234567890"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;
        _systemUnderTest.PropertyOwnerName = "Test Test";
        _systemUnderTest.PropertyOwnerTelephoneNumber = "01234567890";

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionModel.PropertyOwnerTelephoneNumber.Should().Be(_systemUnderTest.PropertyOwnerTelephoneNumber);
            sessionModel.PropertyOwnerName.Should().Be(_systemUnderTest.PropertyOwnerName);
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
        _systemUnderTest.ModelState.AddModelError(nameof(PropertyOwnerNameModel), "Enter the property owner's name");

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
