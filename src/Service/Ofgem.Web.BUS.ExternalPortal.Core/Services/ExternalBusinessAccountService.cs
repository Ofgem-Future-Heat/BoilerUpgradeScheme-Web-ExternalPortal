using Ofgem.API.BUS.BusinessAccounts.Client.Interfaces;
using Ofgem.API.BUS.BusinessAccounts.Domain.CommsObjects;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Lib.BUS.APIClient.Domain.Models;
using Ofgem.Lib.BUS.AuditLogging.Domain.Enums;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using System.Security.Claims;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Services;

public class ExternalBusinessAccountService : IExternalBusinessAccountService
{
    /// <summary>
    /// Client for the api.
    /// </summary>
    private readonly IExternalBusinessAccountsAPIClient _externalBusinessAccountsApiClient;
    private readonly ClaimsPrincipal _claimsPrincipal;

    public ExternalBusinessAccountService(IExternalBusinessAccountsAPIClient externalBusinessAccountsApiClient, ClaimsPrincipal claimsPrincipal)
    {
        _externalBusinessAccountsApiClient = externalBusinessAccountsApiClient ?? throw new ArgumentNullException(nameof(externalBusinessAccountsApiClient));
        _claimsPrincipal = claimsPrincipal ?? throw new ArgumentNullException(nameof(claimsPrincipal));
    }

    public async Task<BusinessAccount> ExternalGetBusinessAccountAsync(Guid? businessAccountId)
    {
        return await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient.ExternalGetBusinessAccountById(businessAccountId).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ExternalUserAccount>> ExternalGetBusinessAccountUsersAsync(Guid businessAccountId)
    {
        return await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient.ExternalGetBusinessAccountUsersAsync(businessAccountId).ConfigureAwait(false);
    }

    public async Task<ExternalUserAccount> GetExternalUserAccountByIdAsync(Guid externalUserAccountId)
    {
        return await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient
                        .GetExternalUserAccountById(externalUserAccountId)
                        .ConfigureAwait(false);
    }

    public async Task<List<ExternalUserAccount>> UpdateExternalUserAccountsAsync(List<ExternalUserAccount> externalUsers, Guid businessAccountId)
    {
        var auditLogParameters = CreateAuditLogParameters(businessAccountId);

        var updateResult = await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient
                                .UpdateExternalUserAccountsAsync(externalUsers, auditLogParameters)
                                .ConfigureAwait(false);
        return updateResult;
    }

    public async Task<List<Invite>> GetInvitesForUserAsync(Guid externalUserAccountId)
    {
        return await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient
                        .GetInvitesForUserAsync(externalUserAccountId)
                        .ConfigureAwait(false);
    }

    public async Task<Invite> UpdateInviteAsync(Invite invite, Guid inviteId)
    {
        return await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient
                        .UpdateInviteAsync(invite, inviteId)
                        .ConfigureAwait(false);
    }

    public async Task<List<InviteStatus>> GetAllInviteStatusAsync()
    {
        return await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient
                        .GetAllInviteStatusAsync()
                        .ConfigureAwait(false);
    }

    private AuditLogParameters CreateAuditLogParameters(Guid? applicationId = null)
    {
        var currentUsername = _claimsPrincipal.GetUsername();

        return new AuditLogParameters
        {
            EntityReferenceId = applicationId,
            Username = currentUsername,
            UserType = AuditLogUserType.External.ToString()
        };
    }

    public async Task<Invite> GetInviteAsync(Guid inviteID)
    {
        return await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient.GetInviteAsync(inviteID);
    }

    public async Task<TokenVerificationResult> VerifyTokenAsync(string token)
    {
        return await _externalBusinessAccountsApiClient.ExternalBusinessAccountRequestsClient.VerifyTokenAsync(token);
    }
}
