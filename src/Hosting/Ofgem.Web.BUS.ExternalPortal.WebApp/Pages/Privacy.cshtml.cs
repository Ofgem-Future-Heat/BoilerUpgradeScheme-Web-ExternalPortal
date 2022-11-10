using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }
    }
}