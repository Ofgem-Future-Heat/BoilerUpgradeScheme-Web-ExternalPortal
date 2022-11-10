using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.EPC;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class EpcEvidenceTests : PageModelTestsBase
{
    private EPCEvidenceModel? _epcEvidenceModel;
    private readonly Mock<ILogger<EPCEvidenceModel>> _logger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        base.TestCaseSetUp();
        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        _logger.Reset();
        _epcEvidenceModel = new EPCEvidenceModel(_logger.Object)
        {
            PageContext = CreatePageContext(httpContext)
        };
    }

    [Test]
    public void Ctor_Success_ShouldNotThrow()
    {
        var result = () => new EPCEvidenceModel(_logger.Object);

        result.Should().NotThrow();
    }

    [Test]
    public void Ctor_NullLogger_ShouldThrow()
    {
        var result = () => new EPCEvidenceModel(null!);

        result.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_SessionData_ShouldReturnPage()
    {
        Session.Put("PageModel", new CreateApplicationModel());

        var result = _epcEvidenceModel!.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Test]
    public void OnGet_SessionNull_ShouldRedirect()
    {
        var result = _epcEvidenceModel!.OnGet();

        result.Should().BeOfType<RedirectToPageResult>();
        if (result is RedirectToPageResult)
        {
            (result as RedirectToPageResult)!.PageName.Should().Be(@Routes.Pages.Path.CD151);
        }
    }

    [Test]
    public void OnPost_SessionData_ShouldReturnPage()
    {
        CreateApplicationModel createApplicationModel = new();
        Session.Put("PageModel", createApplicationModel);

        var result = _epcEvidenceModel!.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
        if (result is RedirectToPageResult)
        {
            (result as RedirectToPageResult)!.PageName.Should().Be(@Routes.Pages.Path.CD167);
        }
        var sessionContent = Session.GetOrDefault<CreateApplicationModel>("PageModel");
        sessionContent.Should().BeEquivalentTo(createApplicationModel);
    }

    [Test]
    public void OnPost_SessionNull_ShouldRedirect()
    {
        var result = _epcEvidenceModel!.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
        if (result is RedirectToPageResult)
        {
            (result as RedirectToPageResult)!.PageName.Should().Be(@Routes.Pages.Path.CD151);
        }
    }
}
