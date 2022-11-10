using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Client.Interfaces;
using Ofgem.API.BUS.Applications.Domain;
using System;
using System.Collections.Generic;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Services;

[TestFixture]
public class ExternalReferenceDataServiceTests
{
    //private ExternalReferenceDataService _systemUnderTest;
    //private Mock<IApplicationsAPIClient> _mockApplicationsApiClient;
    //private Mock<IReferenceDataRequestsClient> _mockReferenceDataRequestsClient;

    //private readonly List<TechType> _techTypes = new List<TechType>
    //{
    //    new TechType{ID = Guid.NewGuid(), TechTypeDescription = "Tech type 1"},
    //    new TechType{ID = Guid.NewGuid(), TechTypeDescription = "Tech type 2"},
    //    new TechType{ID = Guid.NewGuid(), TechTypeDescription = "Tech type 3"}
    //};

    //[OneTimeSetUp]
    //public void FixtureSetup()
    //{
    //    _mockReferenceDataRequestsClient = new Mock<IReferenceDataRequestsClient>();
    //    _mockReferenceDataRequestsClient.Setup(m => m.GetTechTypesAsync().Result).Returns(_techTypes);

    //    _mockApplicationsApiClient = new Mock<IApplicationsAPIClient>();
    //    _mockApplicationsApiClient.Setup(x => x.ReferenceDataRequestsClient).Returns(_mockReferenceDataRequestsClient.Object);
    //}

}
