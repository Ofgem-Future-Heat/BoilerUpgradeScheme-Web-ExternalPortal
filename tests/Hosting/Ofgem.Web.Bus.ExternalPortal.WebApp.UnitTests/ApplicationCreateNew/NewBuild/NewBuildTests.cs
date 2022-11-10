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
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.NewBuild;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class NewBuildTests : PageModelTestsBase
{
    private NewBuildModel _systemUnderTest;
    protected Mock<ILogger<NewBuildModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new NewBuildModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new NewBuildModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            IsNewBuild = true
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
            session?.IsNewBuild?.Should().BeTrue();
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
        Session.Clear();

        // Act.
        _ = _systemUnderTest.OnGet();

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.QuestionResponse.Should().BeNull();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_DropOut()
    {
        // Arrange
        // Yes and biomass
        var sessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.BiomassBoiler
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
            sessionModel?.IsNewBuild.Should().BeTrue();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_EPC()
    {
        // Arrange
        // No and no biomass or heat pump
        var sessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.AirSourceHeatPump
        };
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
            sessionModel?.IsNewBuild.Should().BeFalse();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_Eligible()
    {
        // Arrange
        // Yes and heat pump
        var sessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.SharedGroundLoopSourceHeatPump
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
            sessionModel?.IsNewBuild.Should().BeTrue();
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

        _systemUnderTest.ModelState.AddModelError(nameof(NewBuildModel), "Tell us whether the property is a new build");

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
    public void OnPost_Should_Clear_SelfBuild_Answers_When_Answer_Changes()
    {
        // Arrange - given a session model where the user has selected "yes" to this step and "yes" to eligible self build
        var initialSessionModel = new CreateApplicationModel
        {
            IsNewBuild = true,
            IsEligibleSelfBuild = true,
            TechTypeId = TechTypes.AirSourceHeatPump
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
            updatedSessionModel.IsNewBuild.Should().Be(_systemUnderTest.QuestionResponse);
            updatedSessionModel.IsEligibleSelfBuild.Should().BeNull();
        }
    }

    [Test]
    public void OnPost_Should_Clear_Epc_Answers_When_Answer_Changes()
    {
        // Arrange - given a session model where the user has selected "no" to this step and has answers to EPC questions
        var initialSessionModel = new CreateApplicationModel
        {
            IsNewBuild = false,
            HasEpc = true,
            EpcReferenceNumber = "12345",
            EpcHasRecommendations = true,
            EpcHasExemptions = false,
            TechTypeId = TechTypes.AirSourceHeatPump
        };

        Session.Put(AbstractFormPage.PageModelSessionKey, initialSessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act - when they change the answer to "yes" and submit the form
        _systemUnderTest.QuestionResponse = true;
        _ = _systemUnderTest.OnPost();

        // Assert - then the EPC responses should be cleared from the session model
        var updatedSessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);
        using (new AssertionScope())
        {
            updatedSessionModel.Should().NotBeNull();
            updatedSessionModel.IsNewBuild.Should().Be(_systemUnderTest.QuestionResponse);
            updatedSessionModel.HasEpc.Should().BeNull();
            updatedSessionModel.EpcReferenceNumber.Should().BeNullOrEmpty();
            updatedSessionModel.EpcHasExemptions.Should().BeNull();
            updatedSessionModel.EpcHasRecommendations.Should().BeNull();
        }
    }
}
