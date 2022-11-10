using Ofgem.Lib.BUS.OSPlaces.Client.Domain.DTOs.Responses;

namespace Ofgem.Web.BUS.ExternalPortal.Domain.DTOs;

/// <summary>
/// Represents a result from the postcode lookup
/// </summary>
public class PostcodeLookupResult
{
    /// <summary>
    /// The total number of results returned for the postcode.
    /// </summary>
    public int TotalResults { get; set; } = 0;

    /// <summary>
    /// The total number of results after filtering addresses.
    /// </summary>
    public int FilteredResults { get => Addresses.Count(); }

    /// <summary>
    /// Collection of addresses.
    /// </summary>
    public IEnumerable<AddressResult> Addresses { get; set; } = Enumerable.Empty<AddressResult>();
}
