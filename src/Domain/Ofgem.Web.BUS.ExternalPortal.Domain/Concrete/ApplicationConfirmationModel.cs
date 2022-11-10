namespace Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;

/// <summary>
/// Model for passing application data to the confirmation page. 
/// </summary>
public class ApplicationConfirmationModel
{
    /// <summary>
    /// The human-readable application reference number
    /// </summary>
    public string ApplicationReferenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// The unique DB identifier
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// The property owner's email address
    /// </summary>
    public string? PropertyOwnerEmailAddress { get; set; }

    /// <summary>
    /// The installer's email address
    /// </summary>
    public string InstallerEmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Flag to indicate if the property owner requires manual consent (AD/Welsh)
    /// </summary>
    public bool IsManualConsent { get; set; }

    /// <summary>
    /// Flag to indicate if the property is a self build
    /// </summary>
    public bool IsEligibleSelfBuild { get; set; }

    /// <summary>
    /// Flag to indicate if the property has EPC exemptions
    /// </summary>
    public bool EpcHasExemptions { get; set; }

    /// <summary>
    /// Flag to indicate if Welsh Consent
    /// </summary>
    public bool IsWelshConsent { get; set; }

    

}
