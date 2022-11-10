using Ofgem.API.BUS.BusinessAccounts.Domain.CommsObjects;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;

public interface IExternalBusinessAccountService
{
    /// <summary>
    /// Gets a business account and associated sub-objects.
    /// </summary>
    /// <param name="businessAccountId">Business account ID.</param>
    /// <returns>A <see cref="BusinessAccount"/></returns>
    public Task<BusinessAccount> ExternalGetBusinessAccountAsync(Guid? businessAccountId);

    /// <summary>
    /// Gets user of the business account.
    /// </summary>
    /// <param name="businessAccountId">Business account ID.</param>
    /// <returns>A List of <see cref="ExternalUserAccounts"/> objects</returns>
    public Task<IEnumerable<ExternalUserAccount>> ExternalGetBusinessAccountUsersAsync(Guid businessAccountId);

    /// <summary>
    /// Get external user account with Id.
    /// </summary>
    /// <param name="externalUserAccountId">External user account Id.</param>
    /// <returns><see cref="ExternalUserAccount"/> object.</returns>
    public Task<ExternalUserAccount> GetExternalUserAccountByIdAsync(Guid externalUserAccountId);

    /// <summary>
    /// Update external user accounts.
    /// </summary>
    /// <param name="externalUsers">A List of <see cref="ExternalUserAccount"/> objects to be updated.</param>
    /// <param name="businessAccountId">Business account Id.</param>
    /// <returns>An upadated list of <see cref="ExternalUserAccount"/> objects.</returns>
    public Task<List<ExternalUserAccount>> UpdateExternalUserAccountsAsync(List<ExternalUserAccount> externalUsers, Guid businessAccountId);

    public Task<Invite> GetInviteAsync(Guid inviteID);
    public Task<TokenVerificationResult> VerifyTokenAsync(string token);

    /// <summary>
    /// Get invites for provided external user Id.
    /// </summary>
    /// <param name="externalUserAccountId">External user account Id.</param>
    /// <returns>List of Invites</returns>
    public Task<List<Invite>> GetInvitesForUserAsync(Guid externalUserAccountId);

    /// <summary>
    /// Update invites
    /// </summary>
    /// <param name="invite">Invite object to be updated</param>
    /// <param name="inviteId">Invited Id</param>
    /// <returns>Updated Invite object</returns>
    public Task<Invite> UpdateInviteAsync(Invite invite, Guid inviteId);

    /// <summary>
    /// Retrieve all invite statuses
    /// </summary>
    /// <returns>List of InviteStatus objects</returns>
    public Task<List<InviteStatus>> GetAllInviteStatusAsync();
}