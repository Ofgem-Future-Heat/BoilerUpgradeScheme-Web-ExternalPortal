using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.LeaveFeedback;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class FeedbackDoneTests : PageModelTestsBase
{
    private FeedbackDoneModel? _feedbackDoneModel;
    private readonly Mock<ILogger<FeedbackDoneModel>> _logger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        base.TestCaseSetUp();
        _logger.Reset();
        _feedbackDoneModel = new FeedbackDoneModel(_logger.Object);
    }

    [Test]
    public void Ctor_Success_ShouldNotThrow()
    {
        var result = () => new FeedbackDoneModel(_logger.Object);

        result.Should().NotThrow();
    }

    [Test]
    public void Ctor_NullService_ShouldThrow()
    {
        var result = () => new FeedbackDoneModel(null!);

        result.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
    
    [Test]
    public void OnPostHomeButton_ShouldRedirect()
    {
        var result = _feedbackDoneModel!.OnPostHomeButton();

        result.Should().BeOfType<RedirectToPageResult>();
        if (result is RedirectToPageResult)
        {
            (result as RedirectToPageResult)!.PageName.Should().Be("/ApplicationsDashboard/InstallerApplications");
        }
    }
}
