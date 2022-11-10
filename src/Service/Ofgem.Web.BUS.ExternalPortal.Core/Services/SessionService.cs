using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Services;

/// <summary>
/// Wrapper around <see cref="ISession"/>
/// </summary>
public class SessionService
{
    private readonly ISession _session;

    public SessionService(IHttpContextAccessor accessor, ClaimsPrincipal claimsPrincipal)
    {
        _session = accessor.HttpContext!.Session;

        if (claimsPrincipal.Identity?.IsAuthenticated ?? false)
        {
            var busId = claimsPrincipal.FindFirstValue(B2CClaimTypesConstants.ClaimTypeBusinessAccountId) ?? string.Empty;
            var userId = claimsPrincipal.FindFirstValue(B2CClaimTypesConstants.ClaimTypeExternalUserId) ?? string.Empty;

            UserId = !string.IsNullOrWhiteSpace(userId) ? Guid.Parse(userId) : Guid.Empty;
            BusinessAccountId = !string.IsNullOrWhiteSpace(busId) ? Guid.Parse(busId) : Guid.Empty;
            UserEmail = claimsPrincipal.FindFirstValue(B2CClaimTypesConstants.ClaimTypeEmailAddress) ?? string.Empty;
        }
        else
        {
            UserId = Guid.Empty;
            BusinessAccountId = Guid.Empty;
            UserEmail = string.Empty;
        }
    } 

    public string? Get(string sessionKey)
    {
        return _session.GetString(sessionKey ?? string.Empty);
    }

    public Guid? UserId
    {
        get => _session.GetOrDefault<Guid>("UserId");
        set => _session.Put("UserId", value);
    }

    public string? UserEmail
    {
        get => _session.GetString("UserEmail") ?? string.Empty;
        set => _session.SetString("UserEmail", value ?? string.Empty);
    }

    public Guid? BusinessAccountId
    {
        get => _session.GetOrDefault<Guid>("BusinessAccountId");
        set => _session.Put("BusinessAccountId", value);
    }

    public string ReferenceNumber
    {
        get => _session.GetString("ReferenceNumber") ?? string.Empty;
        set => _session.SetString("ReferenceNumber", value ?? string.Empty);
    }

    public string SearchBy
    {
        get => _session.GetString("SearchBy") ?? string.Empty;
        set => _session.SetString("SearchBy", value ?? string.Empty);
    }

    public string SelectedFilterValue
    {
        get => _session.GetString("SelectedFilterValue") ?? string.Empty;
        set => _session.SetString("SelectedFilterValue", value ?? string.Empty);
    }

    public string SelectedFilterKey
    {
        get => _session.GetString("SelectedFilterKey") ?? string.Empty;
        set => _session.SetString("SelectedFilterKey", value ?? string.Empty);
    }

    public bool IsAllowedCreateNewCase 
    {
        get => _session.GetOrDefault<bool>("IsAllowedCreateNewCase");
        set => _session.Put("IsAllowedCreateNewCase", value);
    }
}
