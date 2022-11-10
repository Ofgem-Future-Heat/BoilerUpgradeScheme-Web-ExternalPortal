using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Extensions;

[TestFixture]
public class ModelStateExtensionsTests
{
    [Test]
    public void HasError_Returns_True_If_Error_Exists()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("TestError", "Something broke");

        // Act
        var isValid = modelState.HasError("TestError");

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void HasError_Returns_False_If_Error_Doesnt_Exist()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Error1", "Something broke");
        modelState.AddModelError("Error2", "Something broke");
        modelState.AddModelError("Error3", "Something broke");

        // Act
        var isValid = modelState.HasError("Error4");

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void HasError_Returns_False_If_ModelState_Empty()
    {
        // Arrange
        var modelState = new ModelStateDictionary();

        // Act
        var isValid = modelState.HasError("Error4");

        // Assert
        isValid.Should().BeFalse();
    }
}
