using Ofgem.API.BUS.Applications.Client.Interfaces;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using Ofgem.Lib.BUS.APIClient.Domain.Models;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Lib.BUS.AuditLogging.Domain.Enums;
using System.Security.Claims;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Services;


/// <summary>
/// Implements the <see cref="IApplicationsAPIClient"/>.
/// </summary>
public class FeedbackService : IFeedbackService
{
    /// <summary>
    /// Client for the api.
    /// </summary>
    private readonly IApplicationsAPIClient _applicationsAPIClient;
    

    public FeedbackService(IApplicationsAPIClient applicationsAPIClient)
    {
        _applicationsAPIClient = applicationsAPIClient ?? throw new ArgumentNullException(nameof(applicationsAPIClient));
    }

    public async Task<bool> StoreFeedback(StoreServiceFeedbackRequest feedback, ClaimsPrincipal currentUserClaimsPrincipal)
    {
        var isSuccess = false;

        if (feedback != null)
        {
            try
            {                
                var auditParams = CreateAuditLogParameters(feedback.ApplicationID, currentUserClaimsPrincipal);
                
                var feedbackList = new Dictionary<string, string>()
                {
                    { "ApplicationId", feedback.ApplicationID.ToString()},
                    { "FeedbackNarrative" , feedback.FeedbackNarrative },
                    { "SurveyOption" , feedback.SurveyOption.ToString() },
                    { "ServiceUsed" , auditParams.UserType }
                };

                await _applicationsAPIClient.ApplicationsRequestsClient.StoreServiceFeedback(feedbackList, auditParams);

                isSuccess = true;
            }
            catch
            {
                return false;
            }
        }

        return isSuccess;
    }
    public async Task<bool> GetApplicationForFeedback(Guid applicationId, ClaimsPrincipal currentUserClaimsPrincipal)
    {
        var isSuccess = true;

        try
        {
            var application = await _applicationsAPIClient.ApplicationsRequestsClient.GetApplicationByIdAsync(applicationId);

            if (application == null)
            {
                isSuccess = false;
            }
            else
            {
                var externalBusinessAccountId = currentUserClaimsPrincipal.FindFirstValue(B2CClaimTypesConstants.ClaimTypeBusinessAccountId);
                var externalBusAccountIdGuid = Guid.Parse(externalBusinessAccountId);
                if (application.BusinessAccountId != externalBusAccountIdGuid)
                {
                    isSuccess = false;
                }
            }

        }
        catch
        {
            return false;
        }

        return isSuccess;
    }

    private static AuditLogParameters CreateAuditLogParameters(Guid? applicationId, ClaimsPrincipal currentUserClaimsPrincipal)
    {
        var currentUsername = currentUserClaimsPrincipal.GetUsername();

        return new AuditLogParameters
        {
            EntityReferenceId = applicationId,
            Username = currentUsername,
            UserType = AuditLogUserType.External.ToString()
        };
    }

}

