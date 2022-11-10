using FluentAssertions;
using NUnit.Framework;
using Ofgem.Web.BUS.ExternalPortal.Core.Validation;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Validation;

[TestFixture]
public class ValidDecimalAttributeTests
{
    [TestCase(1)]
    [TestCase(1.2)]
    [TestCase(1.20)]
    [TestCase(1.23)]
    [TestCase(1000.23)]
    [TestCase("1,000")]
    [TestCase("1,000.23")]
    public void IsValid_Validates_Decimal_Numbers(object input)
    {
        // Arrange
        var validDecimalAttribute = new ValidDecimalAttribute();

        // Act
        var validationResult = validDecimalAttribute.IsValid(input);

        // Assert
        validationResult.Should().BeTrue();
    }

    [TestCase("hello")]
    [TestCase(false)]
    [TestCase(true)]
    [TestCase("1a")]
    public void IsValid_Invalidates_Everything_Else(object input)
    {
        // Arrange
        var validDecimalAttribute = new ValidDecimalAttribute();

        // Act
        var validationResult = validDecimalAttribute.IsValid(input);

        // Assert
        validationResult.Should().BeFalse();
    }
}
