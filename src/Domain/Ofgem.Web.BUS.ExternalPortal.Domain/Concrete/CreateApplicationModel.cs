namespace Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

/// <summary>
/// Model containing values captured during the create application journey.
/// </summary>
public class CreateApplicationModel
{
    public Guid? TechTypeId { get; set; }

    public bool? TechSpecProjectEligible { get; set; }

    public bool? IsSharedGroundLoop { get; set; }

    public bool? BiomassEligible { get; set; }

    public string? InstallationAddressPostcode { get; set; }
    public Address? InstallationAddress { get; set; }
    public bool? InstallationAddressManualEntry { get; set; }


    public bool? IsOnGasGrid { get; set; }

    public bool? IsSocialHousing { get; set; }

    public bool? IsNewBuild { get; set; }

    public bool? IsEligibleSelfBuild { get; set; }

    public bool? HasEpc { get; set; }
    public string? EpcReferenceNumber { get; set; }
    public bool? EpcHasRecommendations { get; set; }
    public bool? EpcHasExemptions { get; set; }

    public string? PropertyOwnerName { get; set; }
    public string? PropertyOwnerTelephoneNumber { get; set; }
    public string? PropertyOwnerEmailAddress { get; set; }
    public bool? CanConsentOnline { get; set; }
    public bool? WelshConsent { get; set; }
    public bool? PropertyOwnerAddressIsInstallationAddress { get; set; }

    public string? PropertyOwnerAddressPostcode { get; set; }
    public Address? PropertyOwnerAddress { get; set; }

    public string? PreviousFuelType { get; set; }
    public string? PreviousFuelTypeOther { get; set; }

    public DateTime? QuoteDate { get; set; }
    public string? QuoteReference { get; set; }
    public decimal? QuoteAmountTotal { get; set; }
    public decimal? QuoteBoilerAmount { get; set; }
}
