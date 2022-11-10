using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.Shared;

/// <summary>
/// DefaultError page model for errors that have a response body redirected through UseExceptionHandler middleware.
/// Right now APIs should only return 500, 404 and 400 but we are not looking at displaying more details about the error
/// at the moment so it makes sense to only show error 500 content through this page.
/// </summary>
public class InternalErrorModel : PageModel
{

    public void OnGet()
    {
        //Can be used to display more details about errors thrown from get requests
    }

    public void OnPost()
    {
        //Can be used to display more details about errors thrown from post requests
    }
}
