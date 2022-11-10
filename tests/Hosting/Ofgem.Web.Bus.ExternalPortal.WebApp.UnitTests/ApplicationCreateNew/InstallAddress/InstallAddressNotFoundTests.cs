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
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.InstallAddress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.ApplicationCreateNew.InstallAddress;

[TestFixture]
public class InstallAddressNotFoundTests : PageModelTestsBase
{
    private readonly ILogger<InstallAddressNotFoundModel> _logger = Mock.Of<ILogger<InstallAddressNotFoundModel>>();
    private InstallAddressNotFoundModel _systemUnderTest = null!;

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        _systemUnderTest = new InstallAddressNotFoundModel(_logger);

        var sessionModel = new CreateApplicationModel
        {
            InstallationAddressPostcode = "AB12 3CD"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        _ = _systemUnderTest.OnGet();

        // Assert
        _systemUnderTest.Postcode.Should().Be(sessionModel.InstallationAddressPostcode);
    }

    [Test]
    public void OnGet_Should_Redirect_To_Postcode_Form_If_Postcode_Not_In_Session()
    {
        // Arrange
        _systemUnderTest = new InstallAddressNotFoundModel(_logger);

        var sessionModel = new CreateApplicationModel();
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        var result = _systemUnderTest.OnGet();

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeOfType<RedirectToPageResult>();
            ((RedirectToPageResult)result).PageName.Should().Be("/ApplicationCreateNew/InstallAddress/InstallPostcode");
        }
    }

    [Test]
    public void OnPost_Should_Redirect_And_Clear_Session()
    {
        // Arrange
        _systemUnderTest = new InstallAddressNotFoundModel(_logger);

        var sessionModel = new CreateApplicationModel
        {
            InstallationAddressPostcode = "AB12 3CD"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var pageHistoryModel = new List<string> { "1", "2", "3" };
        Session.Put(PageHistoryAttribute.PageHistorySessionKey, pageHistoryModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        var redirectUri = "http://www.test.com";

        // Act
        var result = _systemUnderTest.OnPost(redirectUri);

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Be(redirectUri);

            var clearedSessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);
            var clearedPageHistory = Session.GetOrDefault<List<string>>(PageHistoryAttribute.PageHistorySessionKey);

            clearedPageHistory.Should().BeNull();
            clearedSessionModel.Should().BeNull();
        }
    }
}
