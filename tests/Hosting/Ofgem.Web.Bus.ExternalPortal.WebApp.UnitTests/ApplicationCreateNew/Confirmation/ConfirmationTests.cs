using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Confirmation;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Pages.ApplicationCreateNew;

[TestFixture]
public class ConfirmationTests : PageModelTestsBase
{
    private ConfirmationModel? _confirmationModel;
    protected Mock<ILogger<ConfirmationModel>> _logger = new();

    [SetUp]
    public override void TestCaseSetUp()
    {
        base.TestCaseSetUp();
        _logger.Reset();
        _confirmationModel = new ConfirmationModel(_logger.Object);
        var httpContext = new DefaultHttpContext { Session = Session };
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _confirmationModel.TempData = tempData;
    }

    [Test]
    public void Ctor_Success_ShouldNotThrow()
    {
        var result = () => new ConfirmationModel(_logger.Object);

        result.Should().NotThrow();
    }

    [Test]
    public void Ctor_NullLogger_ShouldThrow()
    {
        var result = () => new ConfirmationModel(null!);

        result.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnGet_Success_ShouldLoadValuesFromTempData()
    {
        ApplicationConfirmationModel applicationConfirmationModel = new()
        {
            ApplicationId = Guid.NewGuid(),
            ApplicationReferenceNumber = "A1",
            EpcHasExemptions = false,
            InstallerEmailAddress = "testi@ofgem.gov.uk",
            IsEligibleSelfBuild = false,
            IsManualConsent = false,
            IsWelshConsent = false,
            PropertyOwnerEmailAddress = "testpo@ofgem.gov.uk",
        };
        _confirmationModel!.TempData.Put("ApplicationConfirmationDataModel", applicationConfirmationModel);

        _confirmationModel.OnGet();

        _confirmationModel.ApplicationConfirmationModel.Should().BeEquivalentTo(applicationConfirmationModel);
    }

    [Test]
    public void OnGet_Fail_ShouldThrow()
    {
        var result = () => _confirmationModel!.OnGet();

        result.Should().Throw<Exception>().WithMessage("Could not load application confirmation from temp data");
    }
}
