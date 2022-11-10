using Microsoft.Identity.Web;
using System.Security.Claims;
using System.Security.Principal;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Extensions;

/// <summary>
/// Extends AD functionality to allow more methods
/// </summary>
public static class ADExtension
{
    /// <summary>
    /// Returns the first name from the claims identity for the login partial.
    /// </summary>
    /// <param name="user">Principal user details from AD.</param>
    /// <returns>String of known full name OR if not found a default value of unknown user.</returns>
    public static string GetFullName(this System.Security.Principal.IPrincipal user)
    {
        var identityName = "Unknown user";

        if (user is not null)
        {
            var fullNameClaim = (user.Identity as ClaimsIdentity)?.FindFirst("name");
            if (fullNameClaim is not null)
            {
                identityName = fullNameClaim.Value;
            }
        }

        return identityName;
    }

    /// <summary>
    /// Returns the username claim value from the user's claims identity.
    /// </summary>
    /// <param name="userPrincipal">The user identity principal.</param>
    /// <returns>The user's username, or a default value if the username cannot be found.</returns>
    public static string GetUsername(this IPrincipal userPrincipal)
    {
        var username = "Unknown user";
        var usernameClaimType = "signInNames.emailAddress";

        if (userPrincipal is not null)
        {
            var usernameClaim = (userPrincipal.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.Email)
                                ?? (userPrincipal.Identity as ClaimsIdentity)?.FindFirst(usernameClaimType);
            if (usernameClaim is not null)
            {
                username = usernameClaim.Value;
            }
        }

        return username;
    }

    /// <summary>
    /// Returns the name identifier claim value from the user's claims identity.
    /// </summary>
    /// <param name="userPrincipal">The user identity principal.</param>
    /// <returns>The user ID, or an empty GUID if the claim cannot be found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the userPrincipal parameter is null.</exception>
    public static Guid GetUserId(this IPrincipal userPrincipal)
    {
        if (userPrincipal == null)
        {
            throw new ArgumentNullException(nameof(userPrincipal));
        }

        var userIdentifierClaim = (userPrincipal.Identity as ClaimsIdentity)?.FindFirst(ClaimConstants.NameIdentifierId);
        if (userIdentifierClaim is not null && Guid.TryParse(userIdentifierClaim.Value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }
}