using Ofgem.Web.BUS.ExternalPortal.Domain.DTOs;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;

/// <summary>
/// Handles interactions with the poscode lookup service.
/// </summary>
public interface IPostcodeLookupService
{
    /// <summary>
    /// Gets a list of addresses belonging to a postcode. No filtering is performed.
    /// </summary>
    /// <param name="postcode">The postcode to look up.</param>
    /// <returns>Address data relating to the given postcode.</returns>
    Task<PostcodeLookupResult> GetAddresses(string postcode);

    /// <summary>
    /// Gets a list of addresses in England and Wales belonging to a postcode. 
    /// Addresses outside of England and Wales are filtered out of the response.
    /// </summary>
    /// <param name="postcode">The postcode to look up.</param>
    /// <returns>Address data relating to the given postcode.</returns>
    Task<PostcodeLookupResult> GetEnglishWelshAddresses(string postcode);
}