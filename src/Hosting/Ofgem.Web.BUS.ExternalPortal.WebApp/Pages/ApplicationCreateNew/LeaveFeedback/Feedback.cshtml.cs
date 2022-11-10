using Microsoft.AspNetCore.Mvc;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using Ofgem.Web.BUS.ExternalPortal.Domain.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.ApplicationCreateNew.LeaveFeedback;

public class FeedbackModel : AbstractFormPage
{
    /// <summary>
    /// QuestionResponse - user response.
    /// </summary>
    [Required(ErrorMessage = "Tell us how satisfied you are")]
    [BindProperty]
    public string QuestionResponse { get; set; } = null!;

    [BindProperty]
    [StringLength(1200, ErrorMessage = "Use 1200 characters or fewer")]
    public string? FeedbackText { get; set; } = null!;

    /// <summary>
    /// A list of all errors from client-side validation
    /// </summary>
    public List<string> ErrorMessages { get; set; }
   
    /// <summary>
    /// Interacts with the business client.
    /// </summary>
    private readonly IFeedbackService _feedbackService;

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="logger">Application logging.</param>
    /// <exception cref="ArgumentNullException">Guard statement reaction.</exception>
    public FeedbackModel(ILogger<FeedbackModel> logger,
                                      IFeedbackService feedbackService,
                                      SessionService session)
    {
        _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
        ErrorMessages = new();
    }

    /// <summary>
    public async Task<IActionResult> OnGet()
    {
        var applicationIdString = HttpContext.Request.Query["applictionId"].ToString();

        if (!string.IsNullOrEmpty(applicationIdString))
        {
           var isSuccess =  await _feedbackService.GetApplicationForFeedback(Guid.Parse(applicationIdString), User);
           if (!isSuccess)
           {
                return NotFound();
           }
        }
        else
        {
            return RedirectToPage(@Routes.Pages.Path.CD151);
        }
        
        return this.Page();
    }

    /// <summary>
    /// POST: Consent/Give-feedback
    /// </summary>
    /// <returns>The current page if validation fails. Redirects to the declaration page if successful.</returns>
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessages = new();

            var modelStateErrors = ModelState.SelectMany(x => x.Value!.Errors);
            ErrorMessages.AddRange(modelStateErrors.Select(x => x.ErrorMessage));

            return await OnGet();
        }
        else
        {
            var applicationIdString = HttpContext.Request.Query["applictionId"].ToString();
            var feedbackData = new StoreServiceFeedbackRequest
            {
            ApplicationID = Guid.Parse(applicationIdString),
            FeedbackNarrative = this.FeedbackText ?? String.Empty,
            SurveyOption = Int32.Parse(this.QuestionResponse)
            };

            await _feedbackService.StoreFeedback(feedbackData, User);

            return RedirectToPage("FeedbackDone");
        }
    }

}
