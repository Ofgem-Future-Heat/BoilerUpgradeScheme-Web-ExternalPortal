using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Extensions;

/// <summary>
/// service extensions to add to program.cs
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Configuration and addition business account API and services to Web site - Static helper function.
    /// </summary>
    /// <param name="services">IServiceCollection to build the service at start up.</param>
    /// <param name="configuration">IConfiguration for configuring the service.</param>
    /// <returns>IServiceCollection of a transient service when starting up the generic host.</returns>
    public static IServiceCollection AddRequests(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IExternalBusinessAccountService, ExternalBusinessAccountService>();
        services.AddTransient<IExternalApplicationsService, ExternalApplicationsService>();
        services.AddTransient<IFeedbackService, FeedbackService>(); //added for feedback service

        //services.AddTransient<IExternalReferenceDataService, ExternalReferenceDataService>();

        return services;
    }

    /// <summary>
    /// Adds the service configuration, all new services go here for loading - Static helper function.
    /// </summary>
    /// <param name="services">IServiceCollection to build the service at start up.</param>
    /// <param name="configuration">IConfiguration for configuring the service.</param>
    /// <returns>IServiceCollection of a transient service when starting up the generic host.</returns>
    public static IServiceCollection AddServiceConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<SessionService>();
        services.AddTransient<IPostcodeLookupService, PostcodeLookupService>();
        services.AddTransient<IGraphApiService, GraphApiService>();
        services.AddTransient(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
        return services;
    }

    /// <summary>
    /// Adds Azure AD B2C configuration - Static helper function
    /// </summary>
    /// <param name="services">IServiceCollection to build the service at start up.</param>
    /// <param name="configuration">IConfiguration for configuring the service.</param>
    /// <returns>IServiceCollection of a transient service when starting up the generic host.</returns>
    public static IServiceCollection AddAdb2cConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    configuration.Bind("AzureAdB2C", options);
                    options.Events ??= new OpenIdConnectEvents();
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        var parameters = context.Properties.Items;
                        parameters.Keys.ToList().ForEach(key => context.ProtocolMessage.Parameters.TryAdd(key, parameters[key]));
                        context.ProtocolMessage.Parameters.TryGetValue("action", out var action);
                        if (string.IsNullOrEmpty(action))
                            context.ProtocolMessage.Parameters.TryAdd("action", "signin");
                        return Task.CompletedTask;
                    };
                    options.Events.OnRemoteFailure = context =>
                    {
                        if (context.Failure != null && context.Failure!.Message.Equals("Correlation failed.", StringComparison.InvariantCultureIgnoreCase))
                        {
                            context.HandleResponse();
                            context.Response.Redirect(Routes.Pages.Path.CD155a);
                        }
                        return Task.CompletedTask;
                    };
                });

        services.AddOptions();
        services.Configure<OpenIdConnectOptions>(configuration.GetSection("AzureAdB2C"));
        services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.AccessDeniedPath = new PathString(B2CConstants.AccessDeniedPath);
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
            options.Cookie.MaxAge = TimeSpan.FromHours(24);
            options.Cookie.Name = ".ExternalAspNetCore";
            options.Cookie.IsEssential = true;
        });
        return services;
    }
}