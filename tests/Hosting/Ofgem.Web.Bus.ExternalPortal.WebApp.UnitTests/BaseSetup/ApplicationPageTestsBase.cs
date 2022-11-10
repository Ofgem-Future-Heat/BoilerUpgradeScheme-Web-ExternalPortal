using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.BusinessAccounts.Domain.Entities;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Fakes;
using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Ofgem.Web.BUS.ExternalPortal.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using static Ofgem.API.BUS.BusinessAccounts.Domain.Entities.BusinessAccountSubStatus;
using ApplicationsDomain = Ofgem.API.BUS.Applications.Domain;
using BusinessAccountStatusMappings = Ofgem.API.BUS.BusinessAccounts.Domain.Constants.StatusMappings;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;

public abstract class ApplicationPageTestsBase<TModel> : PageModelTestsBase
{
    private IHttpContextAccessor _httpContextAccessor = null!;
    private static Random rnd = new Random();

    protected Mock<IExternalBusinessAccountService> _mockBusinessAccountService = new();
    protected Mock<IExternalApplicationsService> _mockApplicationsService = new();
    protected Mock<ILogger<TModel>> _mockLogger = new();
    protected SessionService _sessionService = null!;

    protected List<BusinessAccount> _businessAccounts = new();
    protected List<ApplicationsDomain.ApplicationDashboard> _applications = new();


    [OneTimeSetUp]
    public virtual void FixtureSetup()
    {
        var httpContext = new DefaultHttpContext();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        mockHttpContextAccessor.Setup(x => x.HttpContext!.Session).Returns(new FakeHttpSession());

        _httpContextAccessor = mockHttpContextAccessor.Object;
        _sessionService = new SessionService(_httpContextAccessor, Mock.Of<ClaimsPrincipal>());

        _sessionService.BusinessAccountId = Guid.Parse("e8a1efcd-8ef7-4dd3-bac1-aedc38c67221");
        _sessionService.UserId = Guid.NewGuid();
    }

    [SetUp]
    public virtual void TestCaseSetUp()
    {
        _httpContextAccessor.HttpContext!.Session.Clear();
        _businessAccounts = new List<BusinessAccount>();
        _sessionService.BusinessAccountId = Guid.Parse("e8a1efcd-8ef7-4dd3-bac1-aedc38c67221");
        _sessionService.UserId = Guid.NewGuid();
    }

    protected void CreateBusinessAccounts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var setupDay = rnd.Next(1, 30);

            var businessAccount = new BusinessAccount
            {
                Id = Guid.NewGuid(),
                BusinessName = $"Account {i + 1}",
                BusinessAccountNumber = $"BUS-{i + 1}",
                AccountSetupRequestDate = DateTime.Now.AddDays(-setupDay),
                SubStatusId = BusinessAccountStatusMappings.BusinessAccountSubStatus[BusinessAccountSubStatusCode.ACTIV].Id
            };

            _businessAccounts.Add(businessAccount);
        }
    }

    protected void CreateApplications(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var setupDay = rnd.Next(1, 30);

            var application = new ApplicationsDomain.ApplicationDashboard
            {
                ReferenceNumber = $"GID-{i + 1}",
                ReviewRecommendation = "None",
                ApplicationDate = DateTime.Now.AddDays(-setupDay).ToString(),
            };

            _applications.Add(application);
        }
    }
}
