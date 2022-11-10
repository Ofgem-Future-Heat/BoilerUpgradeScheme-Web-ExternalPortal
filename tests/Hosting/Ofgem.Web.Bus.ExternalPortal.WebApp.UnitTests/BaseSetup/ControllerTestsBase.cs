using Microsoft.AspNetCore.Http;
using Ofgem.Web.BUS.ExternalPortal.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup
{
    public abstract class ControllerTestsBase
    {
        protected HttpContext CreateAuthenticatedHttpContext(List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity("custom_auth");
            claimsIdentity.AddClaims(claims);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(claimsIdentity);
            var _httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            return _httpContext;
        }

        protected HttpContext CreateUnAuthenticatedHttpContext(List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaims(claims);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(claimsIdentity);
            var _httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            return _httpContext;
        }
    }
}
