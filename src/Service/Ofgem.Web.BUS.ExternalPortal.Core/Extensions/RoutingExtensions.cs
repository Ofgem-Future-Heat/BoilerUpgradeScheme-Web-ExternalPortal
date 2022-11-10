using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Ofgem.Web.BUS.ExternalPortal.Core.Utilities;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Extensions;

/// <summary>
/// routing extensions to add to program.cs
/// </summary>
public static class RoutingExtensions
{
    /// <summary>
    /// Routing configuration - Static Helper Function
    /// </summary>
    /// <param name="builder">IMvcBuilder to add to the IServiceCollection.</param>
    /// <returns>IMvcBuilder</returns>
    public static IMvcBuilder AddCustomRouting(this IMvcBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.Configure(new Action<RazorPagesOptions>(options =>
        {
            options.Conventions.Add(new DefaultRouteRemovalPageRouteModelConvention(string.Empty));
            options.Conventions.AddPageRoute("/ApplicationsDashboard/InstallerApplications", ""); // Default landing page 

            // CD flow
            foreach (var cd in Routes.RouteList)
            {
                options.Conventions.AddPageRoute(cd.page, cd.route);
            }

            // B2C Redirect Pages
            options.Conventions.AddPageRoute("/B2CRedirect/SignUpB2CRedirect", "sign-up-complete");
            options.Conventions.AddPageRoute("/B2CRedirect/SignInB2CRedirect", "sign-in-complete");
            options.Conventions.AddPageRoute("/B2CRedirect/InviteError", "invite-link-not-working");
            options.Conventions.AddPageRoute("/Account/UserSignedOut", "signed-out");

        }));

        return builder;
    }
}