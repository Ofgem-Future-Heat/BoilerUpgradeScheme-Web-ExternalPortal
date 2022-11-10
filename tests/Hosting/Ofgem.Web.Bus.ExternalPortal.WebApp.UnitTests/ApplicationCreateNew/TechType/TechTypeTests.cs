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
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.TechType;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class TechTypeTests : PageModelTestsBase
{
    private TechTypeModel _systemUnderTest;
    protected Mock<ILogger<TechTypeModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new TechTypeModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new TechTypeModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
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

        // Act.
        _ = _systemUnderTest.OnGet();

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.QuestionResponse.Should().Be(sessionModel.TechTypeId.Value);
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
            _systemUnderTest.QuestionResponse.Should().BeEmpty();
        }
    }

    [Test]
    [TestCase("air", "cae743aa-e0ab-4cfd-9820-300cb1f12074", "Air source heat pump")]
    [TestCase("ground", "490f2536-0b63-4967-9c71-4f968c9f3b05", "Ground source heat pump")]
    [TestCase("biomass", "6113e925-3fbc-4185-8473-75646218c223", "Biomass boiler")]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model(string questionRespone, string techTypeId, string legend)
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;
        _systemUnderTest.QuestionResponse = TechTypes.AirSourceHeatPump;

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        var sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionModel.Should().NotBeNull();
            sessionModel!.TechTypeId.Should().Be(_systemUnderTest.QuestionResponse);
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
        _systemUnderTest.QuestionResponse = Guid.Empty;

        _systemUnderTest.ModelState.AddModelError(nameof(TechTypeModel), "Choose a heating type");

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
    public void OnPost_Should_Reset_Eligibility_Answers_When_TechType_Is_Changed()
    {
        // Arrange - given a session model with a tech type (Biomass) and eligibility steps completed
        var initialSessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.BiomassBoiler,
            TechSpecProjectEligible = true,
            BiomassEligible = true
        };

        Session.Put(AbstractFormPage.PageModelSessionKey, initialSessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act - when the tech type is changed and the form is submitted
        _systemUnderTest.QuestionResponse = TechTypes.AirSourceHeatPump;
        _ = _systemUnderTest.OnPost();

        // Assert - then the subsequent eligibility steps should be reset
        var updatedSessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            updatedSessionModel.Should().NotBeNull();
            updatedSessionModel.TechTypeId.Should().Be(_systemUnderTest.QuestionResponse);
            updatedSessionModel.TechSpecProjectEligible.Should().BeNull();
            updatedSessionModel.IsSharedGroundLoop.Should().BeNull();
            updatedSessionModel.BiomassEligible.Should().BeNull();
        }
    }

    [Test]
    public void OnPost_Should_Not_Reset_Eligibility_Answers_If_TechType_Is_Not_Changed()
    {
        // Arrange - given a session model with a tech type (Biomass) and eligibility steps completed
        var initialSessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.BiomassBoiler,
            TechSpecProjectEligible = true,
            BiomassEligible = true
        };

        Session.Put(AbstractFormPage.PageModelSessionKey, initialSessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act - when the tech type is not changed and the form is submitted
        _systemUnderTest.QuestionResponse = TechTypes.BiomassBoiler;
        _ = _systemUnderTest.OnPost();

        // Assert - then the subsequent eligibility steps should not change
        var updatedSessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);
        using (new AssertionScope())
        {
            updatedSessionModel.Should().NotBeNull();
            updatedSessionModel.TechTypeId.Should().Be(_systemUnderTest.QuestionResponse);
            updatedSessionModel.TechSpecProjectEligible.Should().Be(initialSessionModel.TechSpecProjectEligible);
            updatedSessionModel.IsSharedGroundLoop.Should().Be(initialSessionModel.IsSharedGroundLoop);
            updatedSessionModel.BiomassEligible.Should().Be(initialSessionModel.BiomassEligible);
        }
    }
}
