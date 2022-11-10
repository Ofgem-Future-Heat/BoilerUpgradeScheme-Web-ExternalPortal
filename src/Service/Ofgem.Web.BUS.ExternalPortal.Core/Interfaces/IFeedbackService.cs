using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using System.Security.Claims;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;

public interface IFeedbackService
{
    
    public Task<bool> StoreFeedback(StoreServiceFeedbackRequest feedback, ClaimsPrincipal currentUserClaimsPrincipal);

    public Task<bool> GetApplicationForFeedback(Guid applicationId, ClaimsPrincipal currentUserClaimsPrincipal);

}
