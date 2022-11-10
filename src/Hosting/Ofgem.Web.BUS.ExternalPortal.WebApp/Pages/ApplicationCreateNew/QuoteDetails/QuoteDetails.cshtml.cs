using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ofgem.API.BUS.Applications.Domain;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using Ofgem.Web.BUS.ExternalPortal.Core.Validation;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using Ofgem.Web.BUS.ExternalPortal.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.QuoteDetails;

[PageHistory]
public class QuoteDetailsModel : AbstractFormPage
{
    public Guid TechTypeId { get; set; }

    /// <summary>
    /// Logging for instrumentation of code. 
    /// </summary>
    private readonly ILogger<QuoteDetailsModel> _logger;

    [BindProperty]
    [RegularExpression(@"^\d{0,2}$", ErrorMessage = "Enter the day, month and year as numbers only")]
    public string? QuoteDay { get; set; }

    [BindProperty]
    [RegularExpression(@"^\d{0,2}$", ErrorMessage = "Enter the day, month and year as numbers only")]
    public string? QuoteMonth { get; set; }

    [BindProperty]
    [RegularExpression(@"^\d{0,4}$", ErrorMessage = "Enter the day, month and year as numbers only")]
    public string? QuoteYear { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter a quote reference")]
    [MaxLength(50, ErrorMessage = "Quote reference must be 50 characters or less")]
    public string? QuoteReference { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Enter the quote amount")]
    [RegularExpression(@"(?=.*?\d)^\u00a3?(([1-9]\d{0,2}(,\d{3})*)|\d+)?(\.\d{1,2})?$", ErrorMessage = "Enter the total quote amount, like 16,000.00, 16000.00, 16,000 or 16000")]
    public string? QuoteAmountTotal { get; set; }

    [BindProperty]
    public string? QuoteBoilerAmount { get; set; }


    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public QuoteDetailsModel(ILogger<QuoteDetailsModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// OnGetAsync - Initial page setup and data population.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnGet()
    {
        _logger.LogDebug("QuoteDetails -> OnGet");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        if (sessionModel.TechTypeId != null)
        {
            TechTypeId = sessionModel.TechTypeId.Value;
        }

        if (sessionModel.QuoteDate != null)
        {
            QuoteDay = sessionModel.QuoteDate.Value.Day.ToString();
            QuoteMonth = sessionModel.QuoteDate.Value.Month.ToString();
            QuoteYear = sessionModel.QuoteDate.Value.Year.ToString();
        }

        if (sessionModel.QuoteAmountTotal != null)
        {
            QuoteAmountTotal = String.Format("{0:0.00}", sessionModel.QuoteAmountTotal);

        }

        if (sessionModel.QuoteReference != null)
        {
            QuoteReference = sessionModel.QuoteReference;
        }

        if (sessionModel.QuoteBoilerAmount != null)
        {
            QuoteBoilerAmount = String.Format("{0:0.00}", sessionModel.QuoteBoilerAmount);
        }

        return Page();
    }

    /// <summary>
    /// OnPostAsync - Gets the user decision for the next action.
    /// </summary>
    /// <returns>IAction result to display page.</returns>
    public IActionResult OnPost()
    {
        _logger.LogDebug("QuoteDetails -> OnPost");

        var sessionModel = HttpContext.Session.GetOrDefault<CreateApplicationModel>(PageModelSessionKey);
        if (sessionModel?.TechTypeId == null)
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }

        ValidateDate(nameof(QuoteDay), false);
        var newBoilerAmount = ValidateBoilerAmount(sessionModel.TechTypeId.Value);
        var newTotalAmount = ValidateTotalAmount();


        if (!ModelState.IsValid)
        {
            return OnGet();
        }

        sessionModel.QuoteAmountTotal = newTotalAmount;
        sessionModel.QuoteReference = QuoteReference;
        sessionModel.QuoteBoilerAmount = newBoilerAmount;

        if (DateTime.TryParse($"{QuoteDay}/{QuoteMonth}/{QuoteYear}", out var quoteDate))
        {
            sessionModel.QuoteDate = quoteDate;
        }

        HttpContext.Session.Put(PageModelSessionKey, sessionModel);
        return RedirectToPage(@Routes.Pages.Path.CD171, null, "");
    }

    /// <summary>
    /// Validates date inputs
    /// </summary>
    /// <param name="validationField">Model fieldname to record errors against</param>
    /// <param name="allowFutureDates">Whether to reject dates in the future</param>
    /// <returns><c>true</c> if the date is valid</returns>
    private bool ValidateDate(string validationField, bool allowFutureDates = false)
    {
        if (ModelState.HasError(nameof(QuoteYear))
            || ModelState.HasError(nameof(QuoteMonth))
            || ModelState.HasError(nameof(QuoteDay)))
        {
            return false;
        }

        if (string.IsNullOrEmpty(QuoteYear)
        && string.IsNullOrEmpty(QuoteMonth)
        && string.IsNullOrEmpty(QuoteDay))
        {
            ModelState.AddModelError(validationField, "Enter a date for the quote");
        }

        else if (string.IsNullOrEmpty(QuoteYear)
            || string.IsNullOrEmpty(QuoteMonth)
            || string.IsNullOrEmpty(QuoteDay))
        {
            ModelState.AddModelError(validationField, "Enter a complete date");
        }
        else
        {
            var dateString = $"{QuoteYear} {QuoteMonth} {QuoteDay}";

            if (!(QuoteYear.Length == 4))
            {
                ModelState.AddModelError(validationField, "Enter a year using exactly 4 digits");
            }
                        
            else if (!DateTime.TryParse(dateString, out var dateToValidate))
            {
                ModelState.AddModelError(validationField, "Enter a valid date");
            }


            else if (!allowFutureDates && dateToValidate.Date.CompareTo(DateTime.UtcNow.Date) > 0)
            {
                ModelState.AddModelError(validationField, "Date cannot be in the future");
            }
        }

        return !ModelState.HasError(validationField);
    }
    /// <summary>
    /// Validates the BoilerAmount field. Error messages depend on the technology type selected earlier.
    /// </summary>
    /// <param name="techType">ID of the tech type selected previously.</param>
    private decimal? ValidateBoilerAmount(Guid techType)
    {
        var techTypeName = techType == TechTypes.BiomassBoiler ? "biomass boiler" : "heat pump";

        if (string.IsNullOrWhiteSpace(QuoteBoilerAmount))
        {
            ModelState.AddModelError(nameof(QuoteBoilerAmount), $"Enter the cost of the {techTypeName}");
            return null;

        }

        if (!Regex.IsMatch((QuoteBoilerAmount), @"(?=.*?\d)^\u00a3?(([1-9]\d{0,2}(,\d{3})*)|\d+)?(\.\d{1,2})?$")) {
            ModelState.AddModelError(nameof(QuoteBoilerAmount), $"Enter the cost of just the {techTypeName}, like 6,000.00, 6000.00, 6,000 or 6000");
            return null;
        }

        if (ModelState.HasError(nameof(QuoteBoilerAmount)))
        {
            return null;

        }

        var QuoteBoilerDecimal = decimal.Parse(QuoteBoilerAmount, NumberStyles.Currency);

        if (QuoteBoilerDecimal >= 1_000_000)
        {
            ModelState.AddModelError(nameof(QuoteBoilerAmount), $"Cost of {techTypeName} must be \u00a3999,999.99 or less");
        }

        if (ModelState.HasError(nameof(QuoteAmountTotal)))
        {
            return null;

        }

        var QuoteTotalDecimal = decimal.Parse(QuoteAmountTotal, NumberStyles.Currency);

        if (QuoteBoilerDecimal > QuoteTotalDecimal)
        {
            ModelState.AddModelError(nameof(QuoteBoilerAmount), $"Cost of {techTypeName} cannot be more than total quote amount");
        }
        return QuoteBoilerDecimal;
    }

    private decimal? ValidateTotalAmount()
    {

        if (ModelState.HasError(nameof(QuoteAmountTotal)) || string.IsNullOrWhiteSpace(QuoteAmountTotal))
        {
            return null;
        }
        var QuoteTotalDecimal = decimal.Parse(QuoteAmountTotal, NumberStyles.Currency);


        if (QuoteTotalDecimal >= 1_000_000)
        {
            ModelState.AddModelError(nameof(QuoteAmountTotal), $"Total quote amount must be \u00a3999,999.99 or less");
        }

        return QuoteTotalDecimal;
    }
}

