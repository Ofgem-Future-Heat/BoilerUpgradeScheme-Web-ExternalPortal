using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Offline;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class OfflineTests : PageModelTestsBase
{
    private OfflineModel? _offlineModel;
    private readonly Mock<ILogger<OfflineModel>> _logger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        base.TestCaseSetUp();
        _logger.Reset();
        _offlineModel = new OfflineModel(_logger.Object);
    }

    [Test]
    public void Ctor_Success_ShouldNotThrow()
    {
        var result = () => new OfflineModel(_logger.Object);

        result.Should().NotThrow();
    }

    [Test]
    public void Ctor_NullService_ShouldThrow()
    {
        var result = () => new OfflineModel(null!);

        result.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
    
    [Test]
    public void OnPostHomeButton_ShouldRedirect()
    {
        var result = _offlineModel!.OnGet("contentSelection");

        result.Should().BeOfType<PageResult>();
    }
}
