using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.Text.RegularExpressions;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.DropOut;

[PageHistory]
public class DropOutModel : AbstractFormPage
{
    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<DropOutModel> _logger;
    
    private static readonly Dictionary<string, string> ContentMessages = new()
    {
        { "CD05", "You cannot apply for a grant if the installation address is outside of England and Wales." },
        { "CD09", "You cannot apply for a grant if you're installing a biomass boiler in any kind of new build." },
        { "CD10", "You cannot apply for a grant for a new build unless it is an eligible self build." },
        { "CD12", "You have told us your project does not meet the scheme's eligibility criteria." },
        { "CD29", "A Boiler Upgrade Scheme voucher has already been issued or redeemed for: <p class=\"govuk-body\">$INSTALLATIONADDRESS$ </p>" },
        { "CD106", "You cannot apply for a grant if the installation address is outside of England and Wales." },
        { "CD157", "You have told us your project does not meet the scheme's further requirements for biomass boilers." },
        { "CD158", "You cannot apply for a grant if you're installing a biomass boiler in a property that is connected to the gas grid, including if it has a capped gas supply." },
        { "CD161", "You cannot apply for a grant if the installation property meets the definition of social housing." },
        { "CD162", "The property must have an EPC issued within the last 10 years, unless it is an eligible self build that has not had a previous heating system.</p><p class=\"govuk-body\">You can submit another application after the property gets a new EPC." },
    };

    public string ErrorMessage { get; set; } = null!;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public DropOutModel(ILogger<DropOutModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnGet(string contentSelection)
    {
        // TODO: - To replace with a generic function/extension using Regex later. It should work with others.

        ErrorMessage = TempData != null ? 
            ContentMessages[$"{contentSelection}"].Replace("$INSTALLATIONADDRESS$", TempData.Get<string>("installationAddress")) : 
            ContentMessages[$"{contentSelection}"];

        /*IF ‘No’ to CD12
        You have told us your project does not meet the scheme's eligibility criteria.

        IF ‘No’ to CD157
        You have told us your project does not meet the scheme's further requirements for biomass boilers.

        IF an active voucher already exists for the installation address
        OR IF a BUS voucher has already been paid for the installation address
        (from CD29)
        An Boiler Upgrade Scheme voucher has already been issued or redeemed for this address.

        IF postcode is outside England and Wales on CD5 or CD106
        You cannot apply for a grant if the installation address is outside of England and Wales.

        IF biomass boiler and ‘Yes’ to CD158
        You cannot apply for a grant if you're installing a biomass boiler in a property that is connected to the gas grid, including if it has a capped gas supply.

        IF ‘Yes’ to CD161
        You cannot apply for a grant if the installation property is being used for social housing.   

        IF biomass boiler and ‘Yes’ to CD09
        You cannot apply for a grant if you're installing a biomass boiler in any kind of new build.

        IF ‘No’ to CD10
        You cannot apply for a grant for a new build unless it is an eligible self build.

        UI logic to call 162 
        IF ‘No’ to CD09 AND ‘No’ to CD162
        The property must have an EPC issued within the last 10 years, unless it is an eligible new build. You can submit another application after the property gets a new EPC.
        */

        return Page();
    }

    /// <summary>
    /// OnPost - gets the user response as to if they wish to cancel the application or not!
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnPostHomeButton()
    {
        _logger.LogInformation("public IActionResult OnPost()");
        
        ClearPageModel();

        return RedirectToPage(@Routes.Pages.Path.CD155a);
    }
    
}
