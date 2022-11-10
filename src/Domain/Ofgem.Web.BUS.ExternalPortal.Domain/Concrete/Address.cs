using Ofgem.Lib.BUS.OSPlaces.Client.Domain.DTOs.Responses;

namespace Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

public class Address
{
    public Address()
    {
    }

    public Address(AddressResult addressResult)
    {
        AddressLine1 = string.Join(", ", new[]
        {
            addressResult.SubBuildingName,
            addressResult.BuildingName,
            addressResult.BuildingNumber,
            addressResult.ThoroughfareName
        }.Where(s => !string.IsNullOrEmpty(s)));
        AddressLine3 = addressResult.PostTown;
        Postcode = addressResult.Postcode;
        UPRN = addressResult.Uprn;
        CountryCode = addressResult.CountryCode;
    }

    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? County { get; set; }
    public string Postcode { get; set; } = string.Empty;
    public string UPRN { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? CountryCode { get; set; }

    public override string ToString()
    {
        return string.Join("<br /> ", new[] { AddressLine1, AddressLine2, AddressLine3, Postcode });
    }
}
