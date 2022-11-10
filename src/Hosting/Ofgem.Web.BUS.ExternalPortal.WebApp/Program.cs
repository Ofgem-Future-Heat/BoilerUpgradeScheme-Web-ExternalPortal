global using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web.UI;
using Ofgem.API.BUS.Applications.Client;
using Ofgem.API.BUS.BusinessAccounts.Client.ServiceExtensions;
using Ofgem.Lib.BUS.Logging;
using Ofgem.Lib.BUS.OSPlaces.Client.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using Ofgem.Web.BUS.ExternalPortal.Core.Filters;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(
    options => options.Connect(new Uri($"https://{builder.Configuration["AppConfig:Name"]}.azconfig.io"), new DefaultAzureCredential())
                      .UseFeatureFlags()
                      .Select(KeyFilter.Any, LabelFilter.Null)
                      .Select(KeyFilter.Any, builder.Configuration["AppConfig:LabelFilter"])
                      .ConfigureRefresh(refresh =>
                      {
                          refresh.Register("Bus:SentinelKey", true).SetCacheExpiration(TimeSpan.FromSeconds(30));
                      }));
// Azure Key Vault Configuration
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());

// Add services to the container.
builder.Services.AddOfgemCloudApplicationInsightsTelemetry();

builder.Services.AddExternalBusinessAccountsAPI(builder.Configuration, "BusinessAccountsApiUrl");
builder.Services.AddExternalApplicationsAPI(builder.Configuration, "ApplicationsApiUrl");
builder.Services.AddApplicationsAPIClient(builder.Configuration, "ApplicationsApiUrl"); //added for feedback service

builder.Services.AddOSPlacesServices(builder.Configuration);

builder.Services.AddRequests(builder.Configuration);
builder.Services.AddServiceConfigurations(builder.Configuration);

builder.Services.AddAzureAppConfiguration();

builder.Services.AddFeatureManagement();

builder.Services.AddDistributedMemoryCache();

// Azure AD B2C Configuration
builder.Services.AddAdb2cConfiguration(builder.Configuration);

// External Portal Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".bus.ext.session";
});

builder.Services.AddControllers();

// Adding and configuring Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/", "ClaimTypeBusinessAccount").AllowAnonymousToFolder("/B2CRedirect").AllowAnonymousToFolder("/Shared").AllowAnonymousToFolder("/Help").AllowAnonymousToFolder("/Account");
    options.Conventions.AuthorizeFolder("/", "ClaimTypeUserAccount").AllowAnonymousToFolder("/B2CRedirect").AllowAnonymousToFolder("/Shared").AllowAnonymousToFolder("/Help").AllowAnonymousToFolder("/Account");
    options.Conventions.AuthorizeFolder("/", "AtLeastOneRole").AllowAnonymousToFolder("/B2CRedirect").AllowAnonymousToFolder("/Shared").AllowAnonymousToFolder("/Help").AllowAnonymousToFolder("/Account");
})
    .AddSessionStateTempDataProvider()
    .AddMvcOptions(options => { })
    .AddMicrosoftIdentityUI();


string[] roles = new string[]
{
    B2CConstants.ExternalUserRoleValue,
};

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId)
        .RequireClaim(B2CClaimTypesConstants.ClaimTypeExternalUserId)
        .RequireClaim(ClaimTypes.Role, roles)
        .RequireRole(roles)
        .Build();

    options.AddPolicy("ClaimTypeBusinessAccount", policy => policy.RequireClaim(B2CClaimTypesConstants.ClaimTypeBusinessAccountId));
    options.AddPolicy("ClaimTypeUserAccount", policy => policy.RequireClaim(B2CClaimTypesConstants.ClaimTypeExternalUserId));
    options.AddPolicy("AtLeastOneRole", policy => policy.RequireRole(roles));
});

builder.Services.AddMvc().AddCustomRouting();

//Custom Action Filters
builder.Services.AddControllers(config =>
{
    config.Filters.Add<CustomActionFilterAttribute>();
}).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    // For errors with no respongse body e.g 404 mainly
    app.UseStatusCodePagesWithReExecute("/Shared/RoutingError/", "?statusCode={0}");
    // For errors with response body e.g failed API calls mainly 500
    app.UseExceptionHandler("/Shared/InternalError");
}
else
{
    // If development use the developer page
    app.UseDeveloperExceptionPage();
}

// Configuring
app.UseAzureAppConfiguration();

app.UseSession();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

var externalUserRoleClaim = new Claim(ClaimTypes.Role, B2CConstants.ExternalUserRoleValue);
// Add static role claim to B2C External User
app.Use(async (context, next) =>
{
    var currentUrl = string.Empty;
    var IsObsolete = context.Session.GetOrDefault<bool>(B2CConstants.IsObsoleteUserKey);
    if (context.User != null && context.User.Identity!.IsAuthenticated && !IsObsolete)
    {
        // Add External User Role claim
        context.User.Identities.FirstOrDefault()!.AddClaim(externalUserRoleClaim);
    }
    await next();
});

app.MapControllers();
app.MapRazorPages();

app.UseAuthentication();
app.UseAuthorization();

app.UseTelemetryMiddleware();

app.Run();

