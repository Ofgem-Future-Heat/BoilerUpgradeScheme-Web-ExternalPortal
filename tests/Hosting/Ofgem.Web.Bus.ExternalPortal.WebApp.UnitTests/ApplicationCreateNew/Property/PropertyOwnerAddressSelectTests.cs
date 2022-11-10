using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Lib.BUS.OSPlaces.Client.Domain.DTOs.Responses;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;
using System.Collections.Generic;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.ApplicationCreateNew.Property;

[TestFixture]
public class PropertyOwnerAddressSelectTests : PageModelTestsBase
{
    private readonly ILogger<PropertyOwnerAddressSelectModel> _logger = Mock.Of<ILogger<PropertyOwnerAddressSelectModel>>();
    private PropertyOwnerAddressSelectModel _systemUnderTest = null!;

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressSelectModel(_logger);

        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerAddress = new Address
            {
                AddressLine1 = "Line 1",
                AddressLine2 = "Line 2",
                Postcode = "AB12 3CD",
                UPRN = "12345"
            },
            PropertyOwnerAddressPostcode = "AB12 3CD"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var sessionAddresses = new List<AddressResult>
        {
            new AddressResult { Address = "Address 1", Uprn = "1" }
        };
        Session.Put(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey, sessionAddresses);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        _ = _systemUnderTest.OnGet();

        // Assert
        _systemUnderTest.SelectedAddressUprn.Should().Be(sessionModel.PropertyOwnerAddress.UPRN);
    }

    [Test]
    public void OnGet_Should_Leave_Empty_Form_Values_When_No_Model_In_Session()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressSelectModel(_logger);

        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerAddressPostcode = "AB12 3CD"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var sessionAddresses = new List<AddressResult>
        {
            new AddressResult { Address = "Address 1", Uprn = "1" }
        };
        Session.Put(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey, sessionAddresses);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        _ = _systemUnderTest.OnGet();

        // Assert
        _systemUnderTest.SelectedAddressUprn.Should().BeEmpty();
    }

    [Test]
    public void OnGet_Should_Redirect_To_Postcode_Form_If_Addresses_Not_In_Session()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressSelectModel(_logger);

        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerAddress = new Address
            {
                AddressLine1 = "Line 1",
                AddressLine2 = "Line 2",
                Postcode = "AB12 3CD",
                UPRN = "12345"
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
        var result = _systemUnderTest.OnGet();

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeOfType<RedirectToPageResult>();
            ((RedirectToPageResult)result).PageName.Should().Be("/ApplicationCreateNew/Property/PropertyOwnerPostcode");
        }
    }

    [Test]
    public void OnGet_Should_Map_Addresses_From_Session_To_SelectList()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressSelectModel(_logger);

        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerAddressPostcode = "AB12 3CD"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var sessionAddresses = new List<AddressResult>
        {
            new AddressResult { Address = "Address 1", Uprn = "1" },
            new AddressResult { Address = "Address 2", Uprn = "2" },
            new AddressResult { Address = "Address 3", Uprn = "3" },
            new AddressResult { Address = "Address 4", Uprn = "4" }
        };
        Session.Put(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey, sessionAddresses);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        _ = _systemUnderTest.OnGet();

        // Assert
        _systemUnderTest.Addresses.Should().NotBeNullOrEmpty().And.HaveSameCount(sessionAddresses);
    }

    [Test]
    public void OnPost_Should_Return_Current_Page_If_Validation_Errors()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressSelectModel(_logger);

        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerAddressPostcode = "AB12 3CD"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var sessionAddresses = new List<AddressResult>
        {
            new AddressResult { Address = "Address 1", Uprn = "1" },
            new AddressResult { Address = "Address 2", Uprn = "2" },
            new AddressResult { Address = "Address 3", Uprn = "3" },
            new AddressResult { Address = "Address 4", Uprn = "4" }
        };
        Session.Put(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey, sessionAddresses);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        _systemUnderTest.ModelState.AddModelError("TestKey", "Something broke");

        // Act
        var postResult = _systemUnderTest.OnPost();

        // Assert
        using (new AssertionScope())
        {
            postResult.Should().BeOfType<PageResult>();
            _systemUnderTest.Postcode.Should().Be(sessionModel.PropertyOwnerAddressPostcode);
            _systemUnderTest.Addresses.Should().HaveCount(sessionAddresses.Count);
        }
    }

    [Test]
    public void OnPost_Should_Return_Current_Page_If_Selected_Address_Not_In_Session()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressSelectModel(_logger);
        _systemUnderTest.SelectedAddressUprn = "5";

        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerAddressPostcode = "AB12 3CD"
        };
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var sessionAddresses = new List<AddressResult>
        {
            new AddressResult { Address = "Address 1", Uprn = "1" },
            new AddressResult { Address = "Address 2", Uprn = "2" },
            new AddressResult { Address = "Address 3", Uprn = "3" },
            new AddressResult { Address = "Address 4", Uprn = "4" }
        };
        Session.Put(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey, sessionAddresses);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        var postResult = _systemUnderTest.OnPost();

        // Assert
        using (new AssertionScope())
        {
            postResult.Should().BeOfType<PageResult>();
            _systemUnderTest.Postcode.Should().Be(sessionModel.PropertyOwnerAddressPostcode);
            _systemUnderTest.Addresses.Should().HaveCount(sessionAddresses.Count);
            _systemUnderTest.ModelState.Should().ContainKey("SelectedAddressUprn");
        }
    }

    [Test]
    public void OnPost_Should_Add_Selected_Address_To_Session_And_Continue()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerAddressSelectModel(_logger);
        _systemUnderTest.SelectedAddressUprn = "2";

        var sessionModel = new CreateApplicationModel();
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var sessionAddresses = new List<AddressResult>
        {
            new AddressResult { Address = "Address 1", Uprn = "1", BuildingNumber = "1", ThoroughfareName = "Road 1", PostTown = "Town 1", Postcode = "AA1", CountryCode = "E" },
            new AddressResult { Address = "Address 2", Uprn = "2", BuildingNumber = "2", ThoroughfareName = "Road 2", PostTown = "Town 2", Postcode = "AA2", CountryCode = "E" },
            new AddressResult { Address = "Address 3", Uprn = "3", BuildingNumber = "3", ThoroughfareName = "Road 3", PostTown = "Town 3", Postcode = "AA3", CountryCode = "E" },
            new AddressResult { Address = "Address 4", Uprn = "4", BuildingNumber = "4", ThoroughfareName = "Road 4", PostTown = "Town 4", Postcode = "AA4", CountryCode = "E" }
        };
        Session.Put(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey, sessionAddresses);

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
            sessionModel.Should().NotBeNull();
            sessionModel.PropertyOwnerAddress.Should().NotBeNull();
            sessionModel.PropertyOwnerAddress.AddressLine1.Should().ContainAll(new[] { "Road 2" });
            sessionModel.PropertyOwnerAddress.AddressLine3.Should().ContainAll(new[] { "Town 2" });
            sessionModel.PropertyOwnerAddress.Postcode.Should().Contain("AA2");
            sessionModel.PropertyOwnerAddress.UPRN.Should().Be("2");
            postResult.Should().BeOfType<RedirectToPageResult>();
        }
    }
}
