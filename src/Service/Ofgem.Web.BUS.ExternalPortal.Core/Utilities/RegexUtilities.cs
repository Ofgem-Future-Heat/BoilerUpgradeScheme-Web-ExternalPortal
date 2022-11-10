using System.Globalization;
using System.Text.RegularExpressions;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Utilities;

public static class RegexUtilities
{
    /// <summary>
    /// IsValidEmail - validates the format of the string, making sure it's a valid email address.
    /// </summary>
    /// <param name="email">The email address to be validated.</param>
    /// <returns>True/False flag (Success/Fail). </returns>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

            static string DomainMapper(Match match)
            {
                IdnMapping idn = new()
                {
                    AllowUnassigned = false,
                    UseStd3AsciiRules = false
                };
                string domainName = idn.GetAscii(match.Groups[2].Value);
                return $"{match.Groups[1].Value}{domainName}";
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        try
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Parses a string into an acceptable postcode format - removes non alphanumeric characters
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string ParsePostcode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        try
        {
            var parsedString = Regex.Replace(input, @"[^a-zA-Z0-9 ]", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
            parsedString = Regex.Replace(parsedString, @"\s{2}", " ", RegexOptions.None, TimeSpan.FromMilliseconds(200));
            parsedString = parsedString.Trim().ToUpper();

            return parsedString;
        }
        catch (RegexMatchTimeoutException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Parses a string into an acceptable postcode format for presentation layer
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string NormalisePostcode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // Sanitize the string
        var postcode = ParsePostcode(input);
        
        if (!string.IsNullOrWhiteSpace(postcode))
        {
            // Postcode inbound section should always have 3 characters so insert the space before if one does not already exist
            if (!postcode.Contains(' '))
            {
                postcode = postcode.Insert(postcode.Length - 3, " ");
            }
        }
        
        return postcode;
    }
}