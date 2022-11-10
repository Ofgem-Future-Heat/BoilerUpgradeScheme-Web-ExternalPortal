using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.ApplicationCreateNew.Property;

[TestFixture]
public class PropertyOwnerAddressFormTests : PageModelTestsBase
{
    private readonly ILogger<PropertyOwnerAddressFormModel> _logger = Mock.Of<ILogger<PropertyOwnerAddressFormModel>>();
    private PropertyOwnerAddressFormModel _systemUnderTest = null!;

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressFormModel(_logger);

        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerAddress = new Address
            {
                AddressLine1 = "Address 1",
                AddressLine2 = "Address 2",
                AddressLine3 = "Address 3",
                County = "County",
                Postcode = "AB12 3CD",
                Country = "United Kingdom"
            }
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
        _systemUnderTest.AddressLine1.Should().Be(sessionModel.PropertyOwnerAddress.AddressLine1);
        _systemUnderTest.AddressLine2.Should().Be(sessionModel.PropertyOwnerAddress.AddressLine2);
        _systemUnderTest.Town.Should().Be(sessionModel.PropertyOwnerAddress.AddressLine3);
        _systemUnderTest.County.Should().Be(sessionModel.PropertyOwnerAddress.County);
        _systemUnderTest.Postcode.Should().Be(sessionModel.PropertyOwnerAddressPostcode);
        _systemUnderTest.Country.Should().Be(sessionModel.PropertyOwnerAddress.Country);
    }

    [Test]
    public void OnGet_Should_Leave_Empty_Form_Values_When_No_Model_In_Session()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressFormModel(_logger);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        _ = _systemUnderTest.OnGet();

        // Assert
        _systemUnderTest.AddressLine1.Should().BeNullOrEmpty();
        _systemUnderTest.AddressLine2.Should().BeNullOrEmpty();
        _systemUnderTest.Town.Should().BeNullOrEmpty();
        _systemUnderTest.County.Should().BeNullOrEmpty();
        _systemUnderTest.Postcode.Should().BeNullOrEmpty();
        _systemUnderTest.Country.Should().BeNullOrEmpty();
    }

   
    [Test]
    public void OnPost_Should_Add_Address_To_Session_And_Continue()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressFormModel(_logger)
        {
            AddressLine1 = "Address 1",
            AddressLine2 = "Address 2",
            Town = "Town",
            County = "County",
            Postcode = "Postcode",
            Country = "Country"
        };

        var sessionModel = new CreateApplicationModel();
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        var postResult = _systemUnderTest.OnPost();

        // Assert
        sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            postResult.Should().BeOfType<RedirectToPageResult>();
            sessionModel.Should().NotBeNull();
            sessionModel.PropertyOwnerAddress.Should().NotBeNull();
            sessionModel.PropertyOwnerAddress.AddressLine1.Should().Be(_systemUnderTest.AddressLine1);
            sessionModel.PropertyOwnerAddress.AddressLine2.Should().Be(_systemUnderTest.AddressLine2);
            sessionModel.PropertyOwnerAddress.AddressLine3.Should().Be(_systemUnderTest.Town);
            sessionModel.PropertyOwnerAddress.County.Should().Be(_systemUnderTest.County);
            sessionModel.PropertyOwnerAddress.Postcode.Should().Be(_systemUnderTest.Postcode);
            sessionModel.PropertyOwnerAddress.Country.Should().Be(_systemUnderTest.Country);
        }
    }
}
