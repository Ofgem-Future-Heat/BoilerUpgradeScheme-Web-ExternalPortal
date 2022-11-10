using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Fakes;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.Collections.Generic;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Filters;

[TestFixture]
public class PageHistoryAttributeTests
{
    private class FakePageModel : AbstractFormPage
    {
    }

    private PageHistoryModel pageHistoryModel = new();
    private ISession session = new FakeHttpSession();

    [SetUp]
    public void TestCaseSetUp()
    {
        pageHistoryModel = new PageHistoryModel
        {
            PageHistory = new List<string>
            {
                "/page1",
                "/page2",
                "/page3"
            }
        };
        session = new FakeHttpSession();
    }

    [TestCase("Post")]
    [TestCase("Patch")]
    [TestCase("Delete")]
    public void OnPageHandlerExecuting_Does_Not_Touch_History_If_Method_Is_Not_Get(string httpMethod)
    {
        // Arrange - given a request to a page with the specified HTTP method...
        var systemUnderTest = new PageHistoryAttribute();
        session.Put(PageHistoryAttribute.PageHistorySessionKey, pageHistoryModel);
        var pagePath = "/TestPage";

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.Session).Returns(session);
        mockHttpContext.Setup(m => m.Request.Path).Returns(new PathString(pagePath));
        mockHttpContext.Setup(m => m.Request.Method).Returns(httpMethod);
        var actionContext = CreateActionContext(mockHttpContext.Object);

        var executingContext = new ResultExecutingContext(actionContext,
                                                          new List<IFilterMetadata>(),
                                                          new PageResult(),
                                                          new FakePageModel());

        // Act - ...before any page handler is invoked...
        systemUnderTest.OnResultExecuting(executingContext);

        // Assert - ...the page history should remain untouched because the request method wasn't GET.
        // This simulates a postback to a page.
        var postPageHistory = session.GetOrDefault<PageHistoryModel>(PageHistoryAttribute.PageHistorySessionKey);

        using (new AssertionScope())
        {
            postPageHistory.Should().NotBeNull();
            postPageHistory.PageHistory.Should().NotBeNullOrEmpty().And.BeEquivalentTo(pageHistoryModel.PageHistory, "the request method wasn't GET");
        }
    }

    [Test]
    public void OnPageHandlerExecuting_Does_Not_Touch_History_If_Not_On_Previous_Page()
    {
        // Arrange - given a request to a page via a HTTP GET...
        var systemUnderTest = new PageHistoryAttribute();
        session.Put(PageHistoryAttribute.PageHistorySessionKey, pageHistoryModel);
        var pagePath = "/TestPage";

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.Session).Returns(session);
        mockHttpContext.Setup(m => m.Request.Path).Returns(new PathString(pagePath));
        mockHttpContext.Setup(m => m.Request.Method).Returns("Get");
        var actionContext = CreateActionContext(mockHttpContext.Object);

        var executingContext = new ResultExecutingContext(actionContext,
                                                          new List<IFilterMetadata>(),
                                                          new PageResult(),
                                                          new FakePageModel());

        // Act - ...before any page handler is invoked...
        systemUnderTest.OnResultExecuting(executingContext);

        // Assert - ...the page history should remain untouched we haven't landed on the most recent page in the history.
        // This simulates moving forward a page in the journey.
        var postPageHistory = session.GetOrDefault<PageHistoryModel>(PageHistoryAttribute.PageHistorySessionKey);

        using (new AssertionScope())
        {
            postPageHistory.Should().NotBeNull();
            postPageHistory.CurrentPagePath.Should().Be(pagePath);
            postPageHistory.PageHistory.Should().NotBeNullOrEmpty().And.BeEquivalentTo(pageHistoryModel.PageHistory);
        }
    }

    [Test]
    public void OnPageHandlerExecuting_Removes_Current_Page_From_History()
    {
        // Arrange - given a request to a page via a HTTP GET...
        var systemUnderTest = new PageHistoryAttribute();
        session.Put(PageHistoryAttribute.PageHistorySessionKey, pageHistoryModel);
        var pagePath = "/page3";

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.Session).Returns(session);
        mockHttpContext.Setup(m => m.Request.Path).Returns(new PathString(pagePath));
        mockHttpContext.Setup(m => m.Request.Method).Returns("Get");
        var actionContext = CreateActionContext(mockHttpContext.Object);

        var executingContext = new ResultExecutingContext(actionContext,
                                                          new List<IFilterMetadata>(),
                                                          new PageResult(),
                                                          new FakePageModel());

        // Act - ...before any page handler is invoked...
        systemUnderTest.OnResultExecuting(executingContext);

        // Assert - ...the page history should not contain the page URL.
        // This simulates moving back a page in the journey.
        var postPageHistory = session.GetOrDefault<PageHistoryModel>(PageHistoryAttribute.PageHistorySessionKey);

        using (new AssertionScope())
        {
            postPageHistory.Should().NotBeNull();
            postPageHistory.CurrentPagePath.Should().Be(pagePath);
            postPageHistory.PageHistory.Should().NotBeNullOrEmpty().And.NotContain(pagePath);
        }
    }

    private ActionContext CreateActionContext(HttpContext httpContext)
    {
        return new ActionContext(httpContext,
                                 new RouteData(),
                                 new ActionDescriptor(),
                                 new ModelStateDictionary());
    }

}
