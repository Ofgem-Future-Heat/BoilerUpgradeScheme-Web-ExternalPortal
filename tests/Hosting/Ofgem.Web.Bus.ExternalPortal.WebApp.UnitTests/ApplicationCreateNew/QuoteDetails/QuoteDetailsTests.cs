using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.QuoteDetails;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class QuoteDetailsTests : PageModelTestsBase
{
    private QuoteDetailsModel _systemUnderTest;
    protected Mock<ILogger<QuoteDetailsModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new QuoteDetailsModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new QuoteDetailsModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.AirSourceHeatPump,
            QuoteDate = new DateTime(2022, 1, 1)
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
            session?.TechTypeId.Should().Be(sessionModel.TechTypeId);
            session?.QuoteDate.Value.Year.Should().Be(2022);
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
            _systemUnderTest.QuoteYear.Should().BeNull();
        }
    }

    [Test]
    public void OnPost_Should_Errors_From_Missing_Quote_Date()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.AirSourceHeatPump,
            QuoteDate = null
        };

        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        _systemUnderTest.ModelState.AddModelError("QuoteDay", "Missing QuoteDay");
        _systemUnderTest.ModelState.AddModelError("QuoteMonth", "Missing QuoteMonth");
        _systemUnderTest.ModelState.AddModelError("QuoteYear", "Missing QuoteYear");
        _systemUnderTest.ModelState.AddModelError("QuoteBoilerAmount", "Missing QuoteBoilerAmount");

        _systemUnderTest.ModelState.AddModelError(nameof(QuoteDetailsModel), "Missing quote details - quote date");

        // Act.
        var postResult = _systemUnderTest.OnPost();

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.ModelState.IsValid.Should().BeFalse();
            postResult.Should().BeOfType<PageResult>();
        }
    }

    [Test]
    public void OnPost_Should_Errors_From_Missing_Quote_Boiler_Amount()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.AirSourceHeatPump,
            QuoteDate = new DateTime(2022, 1, 1)
        };

        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        _systemUnderTest.ModelState.AddModelError("QuoteDay", "Missing QuoteDay");
        _systemUnderTest.ModelState.AddModelError("QuoteMonth", "Missing QuoteMonth");
        _systemUnderTest.ModelState.AddModelError("QuoteYear", "Missing QuoteYear");
        _systemUnderTest.ModelState.AddModelError("QuoteBoilerAmount", "Missing QuoteBoilerAmount");

        _systemUnderTest.ModelState.AddModelError(nameof(QuoteDetailsModel), "Missing quote details - quote date");

        _systemUnderTest.QuoteYear = "2022";
        _systemUnderTest.QuoteMonth = "01";
        _systemUnderTest.QuoteDay = "01";

        // Act.
        var postResult = _systemUnderTest.OnPost();

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.ModelState.IsValid.Should().BeFalse();
            postResult.Should().BeOfType<PageResult>();
        }
    }

    [Test]
    public void OnPost_Should_Add_Page_Model_Values_To_Session_Model_And_GoTo_Check_Your_Answers()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel
        {
            TechTypeId = TechTypes.AirSourceHeatPump,
            QuoteDate = new DateTime(2022, 9, 30),
            QuoteReference = "ABC_REF_NUMBER",
            QuoteBoilerAmount = 90000M,
            QuoteAmountTotal = 8575M
        };

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        _systemUnderTest.ModelState.Clear();

        _systemUnderTest.QuoteYear = "2022";
        _systemUnderTest.QuoteMonth = "09";
        _systemUnderTest.QuoteDay = "23";

        _systemUnderTest.QuoteReference = "ABC_REF_NUMBER";
        _systemUnderTest.QuoteBoilerAmount = "90000";
        _systemUnderTest.QuoteAmountTotal = "8575";

        // Act.
        _ = _systemUnderTest.OnPost();

        // Assert
        using (new AssertionScope())
        {
            sessionModel!.Should().NotBeNull();
            sessionModel!.QuoteAmountTotal.Should().Be(8575);
            sessionModel!.QuoteBoilerAmount.Should().Be(90000);
        }
    }
}
