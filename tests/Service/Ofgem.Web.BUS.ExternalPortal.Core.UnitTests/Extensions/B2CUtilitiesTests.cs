using FluentAssertions;
using NUnit.Framework;
using Ofgem.Web.BUS.ExternalPortal.Core.Utilities;
using System;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Extensions;

[TestFixture]
public class B2CUtilitiesTests
{
    [Test]
    public void IsValidAdb2cId_Should_ReturnTrue_When_Valid_AdB2CId()
    {
        // Arrange
        var adb2cId = Guid.NewGuid().ToString();

        // Act
        var result = B2CUtilities.IsValidAdb2cId(adb2cId);

        // Assert
        result.Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    public void IsValidAdb2cId_Should_ReturnFalse_When_InValid_AdB2CId(string adb2cId)
    {
        // Act
        var result = B2CUtilities.IsValidAdb2cId(adb2cId);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidBusinessAccountId_Should_ReturnTrue_When_Valid_BusinessAccountId()
    {
        // Arrange
        var businessAccountId = Guid.NewGuid().ToString();

        // Act
        var result = B2CUtilities.IsValidBusinessAccountId(businessAccountId);

        // Assert
        result.Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    public void IsValidBusinessAccountId_Should_ReturnFalse_When_InValid_BusinessAccountId(string businessAccountId)
    {
        // Act
        var result = B2CUtilities.IsValidBusinessAccountId(businessAccountId);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidExternalUserId_Should_ReturnTrue_When_Valid_ExternalUserId()
    {
        // Arrange
        var externalUserId = Guid.NewGuid().ToString();

        // Act
        var result = B2CUtilities.IsValidExternalUserId(externalUserId);

        // Assert
        result.Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    public void IsValidExternalUserId_Should_ReturnFalse_When_InValid_ExternalUserId(string externalUserId)
    {
        // Act
        var result = B2CUtilities.IsValidExternalUserId(externalUserId);

        // Assert
        result.Should().BeFalse();
    }
}
