using Microsoft.Extensions.Configuration;
using Ofgem.API.BUS.Applications.Client.Interfaces;
using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using Ofgem.API.BUS.Applications.Domain.Entities.Views;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using Ofgem.Lib.BUS.APIClient.Domain.Models;
using Ofgem.Lib.BUS.AuditLogging.Domain.Enums;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using Ofgem.Web.BUS.ExternalPortal.Domain.Exceptions;
using System.Security.Claims;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Services;

/// <summary>
/// Implements the <see cref="IExternalApplicationsService"/>.
/// </summary>
public class ExternalApplicationsService : IExternalApplicationsService
{
    /// <summary>
    /// Client for the api.
    /// </summary>
    private readonly IExternalApplicationsAPIClient _externalApplicationsApiClient;
    private readonly SessionService _sessionService;

    public ExternalApplicationsService(IExternalApplicationsAPIClient externalApplicationsApiClient, IConfiguration configuration, SessionService sessionService)
    {
        _externalApplicationsApiClient = externalApplicationsApiClient ?? throw new ArgumentNullException(nameof(externalApplicationsApiClient));
        _sessionService = sessionService;
    }

    public async Task<IEnumerable<ExternalPortalDashboardApplication>> GetDashboardApplicationsByBusinessAccountIdAsync(Guid businessAccountId, string? searchBy = "", IEnumerable<Guid>? statusIds = null, string? consentState = "") 
        => await _externalApplicationsApiClient.ExternalApplicationsRequestsClient.GetDashboardApplicationsByBusinessAccountIdAsync(businessAccountId, searchBy, statusIds, consentState);

    public async Task<Application?> GetApplicationByReferenceNumberAsync(string referenceNumber)
        => await _externalApplicationsApiClient.ExternalApplicationsRequestsClient.GetApplicationByReferenceNumberAsync(referenceNumber);

    public async Task<Application?> CreateApplicationAsync(CreateApplicationModel model, ClaimsPrincipal currentUserClaimsPrincipal)
    {
        var auditLogParams = CreateAuditLogParameters(null, currentUserClaimsPrincipal);

        var createApplicationRequestModel = new CreateApplicationRequest
        {
            ApplicationDate = DateTime.UtcNow,
            IsBeingAudited = false,
            InstallerUserAccountId = _sessionService.UserId!.Value,
            BusinessAccountID = _sessionService.BusinessAccountId!.Value,
            CreatedBy = currentUserClaimsPrincipal.GetFullName(),
            TechTypeID = model.TechTypeId ?? throw new ModelValidationException("Heating type has not been set", nameof(model.TechTypeId)),
            RuralStatus = model.TechTypeId == TechTypes.BiomassBoiler ? "Rural" : "Not Rural",
            InstallationAddressManualEntry = model.InstallationAddressManualEntry,
            InstallationAddress = model.InstallationAddress != null ?
                new CreateApplicationRequestInstallationAddress
                {
                    Line1 = string.Join(", ", new[]
                    {
                        model.InstallationAddress.AddressLine1,
                        model.InstallationAddress.AddressLine2,
                        model.InstallationAddress.AddressLine3
                    }.Where(line => !string.IsNullOrWhiteSpace(line))),
                    County = model.InstallationAddress.County,
                    Postcode = model.InstallationAddress.Postcode,
                    CountryCode = model.InstallationAddress.CountryCode,
                    UPRN = model.InstallationAddress.UPRN,
                    
                } : throw new ModelValidationException("Installation address has not been set", nameof(model.InstallationAddress)),
            IsGasGrid = model.IsOnGasGrid ?? throw new ModelValidationException("Gas grid has not been set", nameof(model.IsOnGasGrid)),
            IsNewBuild = model.IsNewBuild ?? throw new ModelValidationException("New Build has not been set", nameof(model.IsNewBuild)),
            EpcExists = SetEpcExists(model),
            EpcReferenceNumber = model.EpcReferenceNumber,
            IsLoftCavityExempt = model.EpcHasExemptions,
            PreviousFuelType = model.PreviousFuelType ??
                throw new ModelValidationException("Previous fuel type has not been set", nameof(model.PreviousFuelType)),
            FuelTypeOther = model.PreviousFuelTypeOther,
            PropertyOwnerDetail = new CreateApplicationRequestPropertyOwnerDetail
                {
                    FullName = model.PropertyOwnerName ?? throw new ModelValidationException("Property owner name has not been set", nameof(model.PropertyOwnerName)),
                    TelephoneNumber = model.PropertyOwnerTelephoneNumber ?? throw new ModelValidationException("Property owner telephone number has not been set", nameof(model.PropertyOwnerTelephoneNumber)),
                    Email = model.PropertyOwnerEmailAddress,
                    PropertyOwnerAddressLine1 = model.PropertyOwnerAddress != null ?
                        string.Join(", ", new[]
                        {
                            model.PropertyOwnerAddress.AddressLine1,
                            model.PropertyOwnerAddress.AddressLine2,
                            model.PropertyOwnerAddress.AddressLine3
                        }.Where(line => !string.IsNullOrWhiteSpace(line))) :
                    throw new ModelValidationException("Property owner address has not been set", nameof(model.PropertyOwnerAddress)),
                    PropertyOwnerAddressCounty = model.PropertyOwnerAddress.County,
                    PropertyOwnerAddressPostcode = model.PropertyOwnerAddress.Postcode,
                    PropertyOwnerAddressCountry = model.PropertyOwnerAddress.Country,
                    PropertyOwnerAddressUPRN = model.PropertyOwnerAddress.UPRN
                },
            IsAssistedDigital = model.CanConsentOnline != null ?
                model.CanConsentOnline == false :
                throw new ModelValidationException("Digital assistance has not been set", nameof(model.CanConsentOnline)),
            IsWelshTranslation = model.WelshConsent ?? throw new ModelValidationException("Welsh translation has not been set", nameof(model.WelshConsent)),
            DateOfQuote = model.QuoteDate ?? throw new ModelValidationException("Quote date has not been set", nameof(model.QuoteDate)),
            QuoteAmount = model.QuoteAmountTotal ?? throw new ModelValidationException("Quote amount has not been set", nameof(model.QuoteAmountTotal)),
            TechnologyCost = model.QuoteBoilerAmount ?? throw new ModelValidationException("Technology cost has not been set", nameof(model.QuoteBoilerAmount)),
            QuoteReference = model.QuoteReference ?? throw new ModelValidationException("Quote reference has not been set", nameof(model.QuoteReference)),
            IsSelfBuild = model.IsEligibleSelfBuild
        };

        var createdApplication = await _externalApplicationsApiClient.ExternalApplicationsRequestsClient.CreateApplicationAsync(createApplicationRequestModel, auditLogParams);

        return createdApplication;
    }


    public async Task<bool> CheckDuplicateApplicationAsync(string uprn)
    {
        var checkDuplicateApplication = await _externalApplicationsApiClient.ExternalApplicationsRequestsClient.CheckDuplicateApplicationAsync(uprn, _sessionService.BusinessAccountId!.Value);
        return checkDuplicateApplication;
    }

    public async Task<Voucher> CreateVoucherAsync(Guid applicationId, Guid techTypeId, ClaimsPrincipal currentUserClaimsPrincipal)
    {
        var auditLogParameters = CreateAuditLogParameters(applicationId, currentUserClaimsPrincipal);
        var voucherRequest = new AddVoucherRequest { ApplicationID = applicationId, TechTypeId = techTypeId };
        var voucher = await _externalApplicationsApiClient.ExternalApplicationsRequestsClient.AddVoucherAsync(voucherRequest, auditLogParameters);

        return voucher;
    }

    public async Task<SendConsentEmailResult?> SendConsentEmailAsync(Guid applicationId, ClaimsPrincipal currentUserClaimsPrincipal)
    {
        SendConsentEmailResult sendConsentEmailRequest = new();
        var auditLogParameters = CreateAuditLogParameters(applicationId, currentUserClaimsPrincipal);
        var requestModel = new RequestConsentEmailRequest { CreatedByUsername = currentUserClaimsPrincipal.GetFullName() };

        try
        {
            sendConsentEmailRequest =
               await _externalApplicationsApiClient.ExternalApplicationsRequestsClient.SendConsentEmailAsync(applicationId, requestModel, auditLogParameters);

            return sendConsentEmailRequest;
        }
        catch (Exception)
        {
            return sendConsentEmailRequest;
        }
    }

    private static bool? SetEpcExists(CreateApplicationModel model)
    {
        if (model.IsNewBuild.HasValue && model.IsNewBuild.Value)
        {
            model.HasEpc = null;
        }
        else
        {
            if (!model.HasEpc.HasValue)
            {
                throw new ModelValidationException("EPC selection has not been set", nameof(model.HasEpc));
            }
        }

        return model.HasEpc;
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
