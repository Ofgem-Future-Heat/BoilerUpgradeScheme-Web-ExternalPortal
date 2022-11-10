using Microsoft.AspNetCore.Mvc;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.EPC;

[PageHistory]
public class EPCModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us whether the property has an EPC issued within the last 10 years")]
    [BindProperty]
    public bool? QuestionResponse { get; set; }

    [BindProperty]
    public string? EpcReferenceNumber { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<EPCModel> _logger;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public EPCModel(ILogger<EPCModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnGet()
    {
        _logger.LogInformation("public IActionResult OnGet()");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (sessionModel.HasEpc != null)
        {
            QuestionResponse = sessionModel.HasEpc;
            EpcReferenceNumber = sessionModel.EpcReferenceNumber;
        }

        return Page();
    }

    /// <summary>
    /// OnPostAsync - Gets the user decision for the next action.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnPost()
    {
        _logger.LogInformation("public IActionResult OnPost()");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (QuestionResponse == null)
        {
            return Page();
        }

        if (QuestionResponse == true)
        {
            var newEpcRef = ValidateEPCNumber();
            if (!ModelState.IsValid)
            {
                return OnGet();
            }

            sessionModel.HasEpc = true;
            sessionModel.EpcReferenceNumber = newEpcRef;
            HttpContext.Session.Put(PageModelSessionKey, sessionModel);

            return RedirectToPage(@Routes.Pages.Path.CD164, null, "");
        }

        sessionModel.HasEpc = false;
        HttpContext.Session.Put(PageModelSessionKey, sessionModel);

        return RedirectToPage(@Routes.Pages.Path.CD004, new { ContentSelection = "CD162" });
    }

    /// <summary>
    /// Validate EPC Number - validate the format and content of the ECP referencec number.
    /// </summary>
    private string? ValidateEPCNumber()
    {
        var newEpc = string.Empty;

        if (!string.IsNullOrWhiteSpace(EpcReferenceNumber))
        {
            var trimmedEpcRef = EpcReferenceNumber.Replace("-", "").Replace(" ", "");

            if (!(trimmedEpcRef.Length == 20))
            {
                ModelState.AddModelError(nameof(EpcReferenceNumber), "The EPC report reference number must have 20 digits");
                return newEpc;

            }

            else if (!(trimmedEpcRef.Length == 20 && trimmedEpcRef.All(char.IsDigit)))
            {
                ModelState.AddModelError(nameof(EpcReferenceNumber), "The EPC report reference number can only contain spaces, dashes and numbers");
                return newEpc;

            }

            newEpc = $"{trimmedEpcRef.Substring(0, 4)}-{trimmedEpcRef.Substring(4, 4)}-{trimmedEpcRef.Substring(8, 4)}-{trimmedEpcRef.Substring(12, 4)}-{trimmedEpcRef.Substring(16, 4)}";
        }

        else
        {
            ModelState.AddModelError(nameof(EpcReferenceNumber), "Enter the property's EPC report reference number");

        }

        return newEpc;
    }
}
