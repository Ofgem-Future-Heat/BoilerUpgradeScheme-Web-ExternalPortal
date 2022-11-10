using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.Applications.Domain.Entities.Views;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.Security.Claims;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;

public interface IExternalApplicationsService
{
    /// <summary>
    /// Creates a new BUS application
    /// </summary>
    /// <param name="model">The model containing data from the user.</param>
    /// <param name="currentUserClaimsPrincipal">Claims principal for the current user.</param>
    /// <returns>An <see cref="Application"/> object containing the user's application data.</returns>
    Task<Application?> CreateApplicationAsync(CreateApplicationModel model, ClaimsPrincipal currentUserClaimsPrincipal);

    /// <summary>
    /// Creates a new BUS application
    /// </summary>
    /// <param name="model">The model containing data from the user.</param>
    /// <returns>An <see cref="bool"/> containing if duplicate.</returns>
    Task<bool> CheckDuplicateApplicationAsync(string uprn);

    /// <summary>
    /// Creates a voucher and associates it with the application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="techTypeId">The tech type ID.</param>
    /// <returns>The voucher.</returns>
    Task<Voucher> CreateVoucherAsync(Guid applicationId, Guid techTypeId, ClaimsPrincipal currentUserClaimsPrincipal);

    /// <summary>
    /// Sends a consent request email to the property owner associated with the application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="createdByUsername">The username of the user issuing consent.</param>
    /// <returns>A <see cref="SendConsentEmailResult"/> object indicating if the email has been sent or not..</returns>
    Task<SendConsentEmailResult?> SendConsentEmailAsync(Guid applicationId, ClaimsPrincipal currentUserClaimsPrincipal);

    /// <summary>
    /// Gets a list of application data for a given business account.
    /// </summary>
    /// <param name="businessAccountId">The business account ID.</param>
    /// <param name="searchBy">Optional text field for searching by name, address, reference number.</param>
    /// <param name="statusIds">Optional field for filtering on application or voucher status.</param>
    /// <param name="consentState">Optional field for searching on consent state.</param>
    /// <returns>A list of <see cref="ExternalApplicationDashboard"/> objects containing application data for the current business account.</returns>
    Task<IEnumerable<ExternalPortalDashboardApplication>> GetDashboardApplicationsByBusinessAccountIdAsync(Guid businessAccountId, string? searchBy = "", IEnumerable<Guid>? statusIds = null, string? consentState = "");

    /// <summary>
    /// Finds an application by application reference number.
    /// </summary>
    /// <param name="referenceNumber">The application reference number.</param>
    /// <returns>An <see cref="Application"/>, or null if no application if found.</returns>
    Task<Application?> GetApplicationByReferenceNumberAsync(string referenceNumber);
}
