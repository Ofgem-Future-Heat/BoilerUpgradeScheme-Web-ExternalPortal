using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Cancellation;
using System;
using System.Collections.Generic;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class CancelTests : PageModelTestsBase
{
    private CancelModel _systemUnderTest;
    protected Mock<ILogger<CancelModel>> _mockLogger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        _systemUnderTest = new CancelModel(_mockLogger.Object);
        base.TestCaseSetUp();
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
    {
        // Arrange and Act.
        var action = () => new CancelModel(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OnPost_Should_Exit_Page_Cancel_Page(bool questionResponse)
    {
        // Arrange
        var pageHistoryModel = new PageHistoryModel();
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        _systemUnderTest.QuestionResponse = questionResponse;

        pageHistoryModel.PageHistory.Add($"GoBackToPreviousPage");
        pageHistoryModel.PageHistory.Add($"ExitBackToHomePage");
        Session.Put(PageHistoryAttribute.PageHistorySessionKey, pageHistoryModel);

        // Act.
        var postResult = _systemUnderTest.OnPost();

        // Assert
        using (new AssertionScope())
        {
            if (questionResponse)
            {
                postResult.Should().BeOfType<RedirectToPageResult>();

            }
        }
    }

    [Test]
    public void OnPost_Should_Error_In_Cancel_Page()
    {
        // Arrange
        List<string>? pageHistory = new();
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        _systemUnderTest.QuestionResponse = null;
        _systemUnderTest.ModelState.AddModelError(nameof(CancelModel), "Tell us whether you want to cancel this application.");

        pageHistory.Add($"GoBackToPreviousPage");
        Session.Put(PageHistoryAttribute.PageHistorySessionKey, pageHistory);

        pageHistory = Session.GetOrDefault<List<string>>(PageHistoryAttribute.PageHistorySessionKey);

        // Act.
        _systemUnderTest.OnPost();

        // Assert
        using (new AssertionScope())
        {
            pageHistory.Should().HaveCount(1);
            _systemUnderTest.ModelState.Should().NotBeNull().And.HaveCount(1);
        }
    }
}
