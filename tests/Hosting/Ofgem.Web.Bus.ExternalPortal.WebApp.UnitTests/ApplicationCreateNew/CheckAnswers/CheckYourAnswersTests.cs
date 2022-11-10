using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.CheckAnswers;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.TechType;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class CheckYourAnswersTests : PageModelTestsBase
{
    private CheckYourAnswersModel _systemUnderTest;
    private Mock<ILogger<CheckYourAnswersModel>> _mockLogger = new();
    private Mock<IExternalApplicationsService> _mockApplicationsService = new();
    private Mock<IExternalBusinessAccountService> _mockBusinessAccountService = new();
    private readonly SessionService _sessionService = NewSessionService();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new CheckYourAnswersModel(_mockLogger.Object, _mockApplicationsService.Object, _mockBusinessAccountService.Object, _sessionService);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new CheckYourAnswersModel(null!, _mockApplicationsService.Object, _mockBusinessAccountService.Object, _sessionService);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_ApplicationService_Null()
    {
        // Arrange and Act.
        var action = () => new CheckYourAnswersModel(_mockLogger.Object, null!, _mockBusinessAccountService.Object, _sessionService);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("applicationsService");
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_BusinessAccountService_Null()
    {
        // Arrange and Act.
        var action = () => new CheckYourAnswersModel(_mockLogger.Object, _mockApplicationsService.Object, null!, _sessionService);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("businessAccountService");
    }


    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_SessionService_Null()
    {
        // Arrange and Act.
        var action = () => new CheckYourAnswersModel(_mockLogger.Object, _mockApplicationsService.Object, _mockBusinessAccountService.Object, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("sessionService");
    }

    [Test]
    public void OnPost_Should_Check_Your_Answers_Displayed_On_Page_TechTypeSelection()
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
            _systemUnderTest.CreateApplication.TechTypeId.Should().Be(sessionModel.TechTypeId);
        }
    }

    [Test]
    public void OnPost_Should_Check_Your_Answers_Displayed_On_Page_TechTypeSelection_GroundSource()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.GroundSourceHeatPump
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
            _systemUnderTest.CreateApplication.TechTypeId.Should().Be(sessionModel.TechTypeId);
        }
    }

}
