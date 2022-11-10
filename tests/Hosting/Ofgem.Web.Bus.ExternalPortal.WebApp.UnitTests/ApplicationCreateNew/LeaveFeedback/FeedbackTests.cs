using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Fakes;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.LeaveFeedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class FeedbackTests : PageModelTestsBase
{
    private FeedbackModel? _feedbackModel;
    private readonly Mock<ILogger<FeedbackModel>> _logger = new();
    private readonly Mock<IFeedbackService> _feedbackService = new();
    private SessionService? _sessionService;

    [SetUp]
    public override void TestCaseSetUp()
    {
        base.TestCaseSetUp();
        var httpContext = new DefaultHttpContext();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        var query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "applictionId", Guid.NewGuid().ToString() }
            });
        mockHttpContextAccessor.Setup(x => x.HttpContext!.Request!.Query).Returns(query);
        mockHttpContextAccessor.Setup(x => x.HttpContext!.Session).Returns(new FakeHttpSession());

        _sessionService = new SessionService(mockHttpContextAccessor.Object, Mock.Of<ClaimsPrincipal>());

        _logger.Reset();
        _feedbackModel = new FeedbackModel(_logger.Object, _feedbackService.Object, _sessionService)
        {
            PageContext = CreatePageContext(mockHttpContextAccessor!.Object!.HttpContext!)
        };
    }

    [Test]
    public void Ctor_Success_ShouldNotThrow()
    {
        var result = () => new FeedbackModel(_logger.Object, _feedbackService.Object, _sessionService!);

        result.Should().NotThrow();
    }

    [Test]
    public void Ctor_NullService_ShouldThrow()
    {
        var result = () => new FeedbackModel(_logger.Object, null!, _sessionService!);

        result.Should().Throw<ArgumentNullException>().WithParameterName("feedbackService");
    }
    
    [Test]
    public async Task OnGet_NoApplication_ShouldReturnNotFound()
    {
        _feedbackService.Setup(x => x.GetApplicationForFeedback(It.IsAny<Guid>(), It.IsAny<ClaimsPrincipal>())).ReturnsAsync(false);

        var result = await _feedbackModel!.OnGet();

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task OnGet_ApplicationFound_ShouldReturnPage()
    {
        _feedbackService.Setup(x => x.GetApplicationForFeedback(It.IsAny<Guid>(), It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);

        var result = await _feedbackModel!.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Test]
    public async Task OnGet_NoApplicationId_ShouldRedirect()
    {
        var httpContext = new DefaultHttpContext();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        var query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "X_applictionId", Guid.NewGuid().ToString() }
            });
        mockHttpContextAccessor.Setup(x => x.HttpContext!.Request!.Query).Returns(query);
        mockHttpContextAccessor.Setup(x => x.HttpContext!.Session).Returns(new FakeHttpSession());

        _sessionService = new SessionService(mockHttpContextAccessor.Object, Mock.Of<ClaimsPrincipal>());

        _logger.Reset();
        _feedbackModel = new FeedbackModel(_logger.Object, _feedbackService.Object, _sessionService)
        {
            PageContext = CreatePageContext(mockHttpContextAccessor!.Object!.HttpContext!)
        };

        _feedbackService.Setup(x => x.GetApplicationForFeedback(It.IsAny<Guid>(), It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);

        var result = await _feedbackModel!.OnGet();

        result.Should().BeOfType<RedirectToPageResult>();
        if (result is RedirectToPageResult)
        {
            (result as RedirectToPageResult)!.PageName.Should().Be(@Routes.Pages.Path.CD151);
        }
    }

    [Test]
    public async Task OnPost_Success_ShouldStoreAndRedirect()
    {
        _feedbackModel!.QuestionResponse = "1";
        var result = await _feedbackModel!.OnPost();

        _feedbackService.Verify(x => x.StoreFeedback(It.IsAny<StoreServiceFeedbackRequest>(), It.IsAny<ClaimsPrincipal>()), Times.Once());
        result.Should().BeOfType<RedirectToPageResult>();
        if (result is RedirectToPageResult)
        {
            (result as RedirectToPageResult)!.PageName.Should().Be("FeedbackDone");
        }
    }

    [Test]
    public async Task OnPost_Invalid_ShouldAddErrorMessage()
    {
        _feedbackModel!.QuestionResponse = null!;
        
        foreach (var error in ValidateModel(_feedbackModel))
        {
            _feedbackModel.ModelState.AddModelError(error.ErrorMessage!, error.ErrorMessage!);
        }
        var result = await _feedbackModel!.OnPost();

        _feedbackModel!.ErrorMessages.Count.Should().Be(1);
        _feedbackModel!.ErrorMessages.First().Should().Be("Tell us how satisfied you are");
    }
}
