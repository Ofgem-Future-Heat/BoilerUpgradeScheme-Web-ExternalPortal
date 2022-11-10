using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NUnit.Framework;
using Ofgem.Lib.BUS.OSPlaces.Client.Domain.DTOs.Responses;
using Ofgem.Lib.BUS.OSPlaces.Client.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Services;

[TestFixture]
public class PostcodeLookupServiceTests
{
    private PostcodeLookupService _systemUnderTest = null!;
    private readonly Mock<IPostcodeApiRequestsClient> _mockPostcodeApiRequestsClient = new Mock<IPostcodeApiRequestsClient>();
    private readonly Mock<IOSPlacesApiClient> _mockOSPlacesApiClient = new Mock<IOSPlacesApiClient>();

    private readonly PostcodeResponse _emptyPostcodeResponse = new PostcodeResponse
    {
        Header = new ResponseHeader
        {
            Query = "postcode=AB12",
            Offset = 0,
            MaxResults = 100,
            TotalResults = 0
        },
        Results = new List<DpaResult>()
    };

    private readonly PostcodeResponse _successfulPostcodeResponse = new PostcodeResponse
    {
        Header = new ResponseHeader
        {
            Query = "postcode=AB12",
            Offset = 0,
            MaxResults = 100,
            TotalResults = 4
        },
        Results = new List<DpaResult>
        {
            new DpaResult
            {
                Dpa = new AddressResult
                {
                    Address = "Address 1",
                    Uprn = "1",
                    CountryCode = "E",
                    Postcode = "AB12"
                }
            },
            new DpaResult
            {
                Dpa = new AddressResult
                {
                    Address = "Address 2",
                    Uprn = "2",
                    CountryCode = "S",
                    Postcode = "AB12"
                }
            },
            new DpaResult
            {
                Dpa = new AddressResult
                {
                    Address = "Address 3",
                    Uprn = "3",
                    CountryCode = "W",
                    Postcode = "AB12"
                }
            },
            new DpaResult
            {
                Dpa = new AddressResult
                {
                    Address = "Address 4",
                    Uprn = "4",
                    CountryCode = "E",
                    Postcode = "AB12"
                }
            }
        }
    };

    [OneTimeSetUp]
    public void FixtureSetUp()
    {
        _mockOSPlacesApiClient.Setup(m => m.PostcodeRequestsClient).Returns(_mockPostcodeApiRequestsClient.Object);
    }

    [Test]
    public void Constructor_Throws_ArgumentNullException_If_PostcodeApiClient_Is_Null()
    {
        // Act
        var action = () => new PostcodeLookupService(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("postcodeApiClient");
    }

    [Test]
    public void Constructor_Should_Instantiate_With_Valid_Parameters()
    {
        // Act
        _systemUnderTest = new PostcodeLookupService(_mockOSPlacesApiClient.Object);

        // Assert
        _systemUnderTest.Should().NotBeNull();
    }

    [Test]
    public async Task GetEnglishWelshAddresses_Should_Return_No_Results_If_No_Results_Found_For_Postcode()
    {
        // Arrange
        _mockPostcodeApiRequestsClient.Setup(m => m.GetAddressesForPostcode(It.IsAny<string>()).Result).Returns(_emptyPostcodeResponse);
        _systemUnderTest = new PostcodeLookupService(_mockOSPlacesApiClient.Object);

        // Act
        var result = await _systemUnderTest.GetEnglishWelshAddresses("AB12");

        // Assert
        using (new AssertionScope())
        {
            result.TotalResults.Should().Be(0);
            result.FilteredResults.Should().Be(0);
            result.Addresses.Should().BeNullOrEmpty();
        }
    }

    [Test]
    public async Task GetEnglishWelshAddresses_Should_Filter_Out_Scottish_Results()
    {
        // Arrange
        _mockPostcodeApiRequestsClient.Setup(m => m.GetAddressesForPostcode(It.IsAny<string>()).Result).Returns(_successfulPostcodeResponse);
        _systemUnderTest = new PostcodeLookupService(_mockOSPlacesApiClient.Object);
        var regex = new Regex($@"{"AB12"}(?!\S)");
        var expectedAddresses = _successfulPostcodeResponse.Results.Where(r => r.Dpa.CountryCode.ToUpper() == "E" || r.Dpa.CountryCode.ToUpper() == "W");
        expectedAddresses = expectedAddresses.ToArray().Where(x => regex.IsMatch(x.Dpa.PostalAddressCode));
        
        //Act
        var result = await _systemUnderTest.GetEnglishWelshAddresses("AB12");

        // Assert
        using (new AssertionScope())
        {
            result.TotalResults.Should().Be(expectedAddresses.Count());
            result.FilteredResults.Should().Be(expectedAddresses.Count());
            result.Addresses.Should().BeNullOrEmpty();
        }
    }

    [Test]
    public async Task GetEnglishWelshAddresses_Incomplete_PostCode_Results()
    {
        // Arrange
        _mockPostcodeApiRequestsClient.Setup(m => m.GetAddressesForPostcode(It.IsAny<string>()).Result).Returns(_successfulPostcodeResponse);
        _systemUnderTest = new PostcodeLookupService(_mockOSPlacesApiClient.Object);
        var regex = new Regex($@"{"AB12"}(?!\S)");
        var expectedAddresses = _successfulPostcodeResponse.Results.Where(r => r.Dpa.CountryCode.ToUpper() == "E" || r.Dpa.CountryCode.ToUpper() == "W");
        expectedAddresses = expectedAddresses.ToArray().Where(x => regex.IsMatch(x.Dpa.PostalAddressCode));

        // Act
        var result = await _systemUnderTest.GetEnglishWelshAddresses("AB12");

        // Assert
        using (new AssertionScope())
        {
            result.TotalResults.Should().Be(expectedAddresses.Count());
            result.FilteredResults.Should().Be(expectedAddresses.Count());
            result.Addresses.Should().BeNullOrEmpty();
        }
    }

    [Test]
    public async Task GetAddresses_Should_Return_No_Results_If_No_Results_Found_For_Postcode()
    {
        // Arange
        _mockPostcodeApiRequestsClient.Setup(m => m.GetAddressesForPostcode(It.IsAny<string>()).Result).Returns(_emptyPostcodeResponse);
        _systemUnderTest = new PostcodeLookupService(_mockOSPlacesApiClient.Object);

        // Act
        var result = await _systemUnderTest.GetAddresses("AB12");

        // Assert
        using (new AssertionScope())
        {
            result.TotalResults.Should().Be(0);
            result.FilteredResults.Should().Be(0);
            result.Addresses.Should().BeNullOrEmpty();
        }
    }

    [Test]
    public async Task GetAddresses_Should_Return_Results()
    {
        // Arrange
        _mockPostcodeApiRequestsClient.Setup(m => m.GetAddressesForPostcode(It.IsAny<string>()).Result).Returns(_emptyPostcodeResponse);
        _systemUnderTest = new PostcodeLookupService(_mockOSPlacesApiClient.Object);

        // Act
        var result = await _systemUnderTest.GetAddresses("AB12");

        // Assert
        using (new AssertionScope())
        {
            result.TotalResults.Should().Be(0);
            result.FilteredResults.Should().Be(0);
            result.Addresses.Should().BeNullOrEmpty();
        }
    }
}
