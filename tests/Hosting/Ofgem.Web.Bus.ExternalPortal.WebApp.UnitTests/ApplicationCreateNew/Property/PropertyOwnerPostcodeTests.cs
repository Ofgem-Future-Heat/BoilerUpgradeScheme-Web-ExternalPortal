using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Lib.BUS.OSPlaces.Client.Domain.DTOs.Responses;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.DTOs;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.InstallAddress;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.ApplicationCreateNew.Property;

[TestFixture]
public class PropertyOwnerPostcodeTests : PageModelTestsBase
{
    private readonly ILogger<PropertyOwnerPostcodeModel> _logger = Mock.Of<ILogger<PropertyOwnerPostcodeModel>>();
    private readonly Mock<IPostcodeLookupService> _mockPostcodeLookupService = new Mock<IPostcodeLookupService>();
    private PropertyOwnerPostcodeModel _systemUnderTest = null!;

    [Test]
    public void OnGet_Should_Load_Form_Values_From_Session()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerPostcodeModel(_logger, _mockPostcodeLookupService.Object);

        var sessionModel = new CreateApplicationModel
        {
            PropertyOwnerAddressPostcode = "TestPostCode"
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
        _systemUnderTest.PropertyOwnerAddressPostcode.Should().Be(sessionModel.PropertyOwnerAddressPostcode);
    }

    [Test]
    public void OnGet_Should_Leave_Empty_Form_Values_When_No_Model_In_Session()
    {
        // Arrange
        _systemUnderTest = new PropertyOwnerPostcodeModel(_logger, _mockPostcodeLookupService.Object);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        _ = _systemUnderTest.OnGet();

        // Assert
        _systemUnderTest.PropertyOwnerAddressPostcode.Should().BeEmpty();
    }

    [Test]
    public async Task OnPostAsync_Should_Handle_Error_Thrown_By_Postcode_Lookup()
    {
        // Arrange
        _mockPostcodeLookupService.Setup(m => m.GetEnglishWelshAddresses(It.IsAny<string>()).Result).Throws(new Exception("Something bad happened"));
        _systemUnderTest = new PropertyOwnerPostcodeModel(_logger, _mockPostcodeLookupService.Object)
        {
            PropertyOwnerAddressPostcode = "AB12 3CD"
        };

        // Act
        _ = await _systemUnderTest.OnPostAsync();

        // Assert
        _systemUnderTest.ModelState.Should().NotBeEmpty().And.ContainKey("PropertyOwnerAddressPostcode");
    }

    [Test]
    public async Task OnPostAsync_Should_Add_Postcodes_To_Session_And_Continue()
    {
        // Arrange
        var inputPostcode = "AB12 3CD";

        var sessionModel = new CreateApplicationModel();
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);

        var addressList = new List<AddressResult>
        {
            new AddressResult {Address = "Address1", Uprn = "1"},
            new AddressResult {Address = "Address2", Uprn = "2"},
            new AddressResult {Address = "Address3", Uprn = "3"}
        };

        var postcodeResult = new PostcodeLookupResult { Addresses = addressList, TotalResults = addressList.Count };
        _mockPostcodeLookupService.Setup(m => m.GetAddresses(It.IsAny<string>()).Result).Returns(postcodeResult);
        _systemUnderTest = new PropertyOwnerPostcodeModel(_logger, _mockPostcodeLookupService.Object)
        {
            PropertyOwnerAddressPostcode = inputPostcode,
            PageContext = pageContext
        };

        // Act
        var postResult = await _systemUnderTest.OnPostAsync();

        // Assert
        var sessionAddresses = Session.GetOrDefault<IEnumerable<AddressResult>>(PropertyOwnerPostcodeModel.PropertyOwnerAddressesSessionKey);
        sessionModel = Session.GetOrDefault<CreateApplicationModel>(AbstractFormPage.PageModelSessionKey);

        using (new AssertionScope())
        {
            sessionAddresses.Should().NotBeNullOrEmpty().And.Contain(addressList);
            sessionModel.Should().NotBeNull();
            sessionModel!.PropertyOwnerAddressPostcode.Should().NotBeEmpty().And.Be(inputPostcode);
            postResult.Should().BeOfType<RedirectToPageResult>();
        }
    }

    [Test]
    public async Task OnPostAsync_Should_Redirect_To_Manual_Address_Form_If_No_Results_Returned()
    {
        // Arrange
        var postcodeResult = new PostcodeLookupResult { Addresses = Enumerable.Empty<AddressResult>(), TotalResults = 5 };
        _mockPostcodeLookupService.Setup(m => m.GetAddresses(It.IsAny<string>()).Result).Returns(postcodeResult);
        _systemUnderTest = new PropertyOwnerPostcodeModel(_logger, _mockPostcodeLookupService.Object)
        {
            PropertyOwnerAddressPostcode = "AB12 3CD"
        };

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);
        _systemUnderTest.PageContext = pageContext;

        // Act
        var postResult = await _systemUnderTest.OnPostAsync();

        // Assert
        postResult.Should().BeOfType<RedirectToPageResult>();
    }
    [Test]
    public async Task OnPostAsync_Should_Return_Model_Error_ForPartialPostCode_And_No_Results_Returned()
    {
        // Arrange
        var sessionModel = new CreateApplicationModel();
        Session.Put(AbstractFormPage.PageModelSessionKey, sessionModel);

        var httpContext = new DefaultHttpContext
        {
            Session = Session
        };
        var pageContext = CreatePageContext(httpContext);

        var postcodeResult = new PostcodeLookupResult { Addresses = Enumerable.Empty<AddressResult>(), TotalResults = 0 };
        _mockPostcodeLookupService.Setup(m => m.GetAddresses(It.IsAny<string>()).Result).Returns(postcodeResult);
        _systemUnderTest = new PropertyOwnerPostcodeModel(_logger, _mockPostcodeLookupService.Object)
        {
            PropertyOwnerAddressPostcode = "AB12 3C",
            PageContext = pageContext

        };

        // Act
        _ = await _systemUnderTest.OnPostAsync();

        // Assert
        _systemUnderTest.ModelState.Should().NotBeEmpty();
    }
}
