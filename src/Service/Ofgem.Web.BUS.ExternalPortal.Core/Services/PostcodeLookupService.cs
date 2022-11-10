using Ofgem.Lib.BUS.OSPlaces.Client.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Domain.DTOs;
using System.Text.RegularExpressions;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Services;

/// <summary>
/// Implements <see cref="IPostcodeLookupService"/>.
/// </summary>
public class PostcodeLookupService : IPostcodeLookupService
{
    private readonly IOSPlacesApiClient _postcodeApiClient;

    public PostcodeLookupService(IOSPlacesApiClient postcodeApiClient)
    {
        _postcodeApiClient = postcodeApiClient ?? throw new ArgumentNullException(nameof(postcodeApiClient));
    }
    public async Task<PostcodeLookupResult> GetEnglishWelshAddresses(string postcode)
    {
        var resultValue = new PostcodeLookupResult();
        var strPostcode = Regex.Replace(postcode, @"\s", "");
        var addressResult = await _postcodeApiClient.PostcodeRequestsClient.GetAddressesForPostcode(strPostcode);

        if (addressResult?.Results != null && addressResult.Results.Any())
        {
            var englishWelshResults = addressResult.Results.Where(r => r.Dpa.CountryCode.ToUpper() == "E" || r.Dpa.CountryCode.ToUpper() == "W" && r.Dpa.Language.ToUpper() == "EN");


            resultValue.TotalResults = addressResult.Results.Count();
            resultValue.Addresses = englishWelshResults.Select(result => result.Dpa);

            var searchStr = strPostcode;
            resultValue.Addresses = resultValue.Addresses.AsEnumerable().Where(x => x.Address.StartsWith($"{searchStr} ") ||
                                     x.Address.Contains($" {searchStr} ") ||
                                     x.Address.Contains($".{searchStr} ") ||
                                     x.Address.Contains($" {searchStr}.") ||
                                     x.Address.EndsWith($" {searchStr}") ||
                                     MatchBtwSpace(x.Address, searchStr)).ToList();
            resultValue.TotalResults = resultValue.Addresses.ToList().Count;

        }

        return resultValue;
    }


    public async Task<PostcodeLookupResult> GetAddresses(string postcode)
    {
        var resultValue = new PostcodeLookupResult();
        var strPostcode = Regex.Replace(postcode, @"\s", "");
        var addressResult = await _postcodeApiClient.PostcodeRequestsClient.GetAddressesForPostcode(strPostcode);

        if (addressResult?.Results != null && addressResult.Results.Any())
        {
            var filteredAddressResult = addressResult.Results.Where(r => r.Dpa.Language.ToUpper() == "EN");
            resultValue.TotalResults = filteredAddressResult.Count();
            resultValue.Addresses = filteredAddressResult.Select(result => result.Dpa);

            var searchStr = strPostcode;
            resultValue.Addresses = resultValue.Addresses.AsEnumerable().Where(x => x.Address.StartsWith($"{searchStr} ") ||
                                     x.Address.Contains($" {searchStr} ") ||
                                     x.Address.Contains($".{searchStr} ") ||
                                     x.Address.Contains($" {searchStr}.") ||
                                     x.Address.EndsWith($" {searchStr}") ||
                                     MatchBtwSpace(x.Address, searchStr)).ToList();
            resultValue.TotalResults = resultValue.Addresses.ToList().Count;

        }

        return resultValue;
    }

    private static bool MatchBtwSpace(string str, string query)
    {
        // remove all non alphanumeric characters
        char[] arr = str.Where(c => char.IsLetterOrDigit(c) ||
                                    char.IsWhiteSpace(c)).ToArray();
        str = new string(arr);

        // split to list of words
        var words = str.Split(" ");
        for (int i = 0; i < words.Length; i++)
        {
            if (i + 1 == words.Length)
                break;
            var match = $"{words[i]}{words[i + 1]}" == query;
            if (match)
                return true;
        }

        return false;
    }

}
