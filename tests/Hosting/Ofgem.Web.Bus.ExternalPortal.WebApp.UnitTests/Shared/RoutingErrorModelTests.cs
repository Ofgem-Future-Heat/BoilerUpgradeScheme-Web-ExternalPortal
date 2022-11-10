using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.Shared;
using System.Collections.Generic;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Shared;

[TestFixture]
public class RoutingErrorModelTests
{
    private RoutingErrorModel _systemUnderTest = null!;

    [TestCase(401)]
    [TestCase(403)]
    [TestCase(404)]
    [TestCase(500)]
    [TestCase(503)]
    public void Call_RoutingErrorPage_With_Known_Status_Code(int statusCode)
    {
        // Arrange
        _systemUnderTest = new RoutingErrorModel();

        // Act
        _systemUnderTest.OnGet(statusCode);

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.ErrorTitle.Should().NotBeNullOrEmpty();
            _systemUnderTest.ErrorPageContent.Should().NotBeNullOrEmpty();
        }
    }

    [TestCase(400)]
    public void Call_RoutingErrorPage_With_StatusCode(int statusCode)
    {
        // Arrange
        _systemUnderTest = new RoutingErrorModel();
        var expectedTitle = "There is a problem.";
        var expectedPageContent = new List<string> { "an internal application issue." };

        // Act
        _systemUnderTest.OnGet(statusCode);

        // Assert
        using (new AssertionScope())
        {
            _systemUnderTest.ErrorTitle.Should().Be(expectedTitle);
            _systemUnderTest.ErrorPageContent.Should().Contain(expectedPageContent);
        }
    }
}