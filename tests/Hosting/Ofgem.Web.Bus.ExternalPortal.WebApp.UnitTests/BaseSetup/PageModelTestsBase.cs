using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Fakes;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;

public abstract class PageModelTestsBase
{
    protected static ISession Session = new FakeHttpSession();

    [SetUp]
    public virtual void TestCaseSetUp()
    {
        Session = new FakeHttpSession();
    }

    protected IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    protected ITempDataDictionary ConfigureTempData()
    {
        var tempDataProvider = Mock.Of<ITempDataProvider>();
        var factory = new TempDataDictionaryFactory(tempDataProvider);
        var tempData = factory.GetTempData(new DefaultHttpContext());

        return tempData;
    }

    protected PageContext CreatePageContext(HttpContext httpContext)
    {
        return new PageContext(new ActionContext(httpContext,
                                                 new RouteData(),
                                                 new PageActionDescriptor(),
                                                 new ModelStateDictionary()));
    }

    protected static SessionService NewSessionService()
    {
        return new(new HttpContextAccessor()
        {
            HttpContext = new DefaultHttpContext()
            {
                Session = Session
            }
        }, Mock.Of<ClaimsPrincipal>());
    }
}
