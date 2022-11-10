using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.EPC;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class EPCRecommendationsTests : PageModelTestsBase
{
    private EPCRecommendationsModel _systemUnderTest;
    protected Mock<ILogger<EPCRecommendationsModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new EPCRecommendationsModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new EPCRecommendationsModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            EpcHasRecommendations = true
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
            session?.EpcHasRecommendations.Should().BeTrue();
            _systemUnderTest.QuestionResponse.Should().BeTrue();
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

        // Act.
        _ = _systemUnderTest.OnGet();

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.QuestionResponse.Should().BeNull();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_EPC_Recommendations()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            EpcReferenceNumber = "1234-5678-9101-1213-1516"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;
        _systemUnderTest.QuestionResponse = true;

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionModel.Should().NotBeNull();
            _systemUnderTest!.QuestionResponse.Should().BeTrue();
            sessionModel?.EpcHasRecommendations.Should().BeTrue();
            sessionModel?.EpcHasRecommendations.Should().BeTrue();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_FuelType_Used()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel();
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;
        _systemUnderTest.QuestionResponse = false;

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionModel.Should().NotBeNull();
            _systemUnderTest!.QuestionResponse.Should().BeFalse();
            sessionModel?.EpcHasRecommendations.Should().BeFalse();
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
        _systemUnderTest.QuestionResponse = null;

        _systemUnderTest.ModelState.AddModelError(nameof(EPCRecommendationsModel), "Tell us whether the property’s EPC report has recommendations for loft or cavity wall insulation");

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
    public void OnPost_Should_Clear_EpcExemptions_If_Answer_Changes()
    {
        // Arrange - given a session model where the user has selected "yes" and answered the EPC exemptions step
        var initialSessionModel = new CreateApplicationModel
        {
            EpcHasRecommendations = true,
            EpcHasExemptions = true
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
            updatedSessionModel.EpcHasRecommendations.Should().Be(_systemUnderTest.QuestionResponse);
            updatedSessionModel.EpcHasExemptions.Should().BeNull();
        }
    }
}
