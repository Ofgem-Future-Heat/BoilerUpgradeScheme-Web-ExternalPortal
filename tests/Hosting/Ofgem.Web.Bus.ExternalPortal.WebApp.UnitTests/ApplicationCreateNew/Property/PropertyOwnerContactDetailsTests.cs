using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class PropertyOwnerContactDetailsTests : PageModelTestsBase
{
    private PropertyOwnerContactDetailsModel _systemUnderTest;
    protected Mock<ILogger<PropertyOwnerContactDetailsModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new PropertyOwnerContactDetailsModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new PropertyOwnerContactDetailsModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerName = "Test MyName",
            PropertyOwnerTelephoneNumber = "01234567890",
            PropertyOwnerEmailAddress = "test@test.com"
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
        _systemUnderTest.PropertyOwnerEmailAddress.Should().Be(sessionModel.PropertyOwnerEmailAddress);
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
            _systemUnderTest.PropertyOwnerEmailAddress.Should().BeNull();
            _systemUnderTest.QuestionResponse.Should().BeNull();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_Property_Owner_Welsh()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerName = "Test MyName",
            PropertyOwnerTelephoneNumber = "01234567890",
            PropertyOwnerEmailAddress = "test@test.com"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;
        _systemUnderTest.QuestionResponse = true;
        _systemUnderTest.PropertyOwnerEmailAddress = "MyTest@MyTest.com";

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionModel.Should().NotBeNull();
            sessionModel.CanConsentOnline.Should().BeTrue();
            sessionModel.PropertyOwnerEmailAddress.Should().Be(_systemUnderTest.PropertyOwnerEmailAddress);
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
        _systemUnderTest.ModelState.AddModelError(nameof(PropertyOwnerContactDetailsModel), "Tell us whether the property owner can give their consent online");

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
    public void OnPost_Should_Clear_EmailAddress_If_Answer_Changes()
    {
        // Arrange - given a session model where the user has selected "yes" and provided an email address
        var initialSessionModel = new CreateApplicationModel
        {
            CanConsentOnline = true,
            PropertyOwnerEmailAddress = "propertyowner@test.com"
        };

        Session.Put(AbstractFormPage.PageModelSessionKey, initialSessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act - when they change the answer to "no" and submit the form
        _systemUnderTest.QuestionResponse = false;
        _ = _systemUnderTest.OnPost();

        // Assert - then the email address should be cleared from the session model
        var updatedSessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            updatedSessionModel.Should().NotBeNull();
            updatedSessionModel.CanConsentOnline.Should().Be(_systemUnderTest.QuestionResponse);
            updatedSessionModel.PropertyOwnerEmailAddress.Should().BeNullOrEmpty();
        }
    }
}
