using FluentAssertions;
using Microsoft.Identity.Web;
using NUnit.Framework;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Extensions;

[TestFixture]
public class ADExtensionsTests
{
    [Test]
    public void GetFullName_Returns_Name_Claim()
    {
        // Arrange
        var fullNameValue = "Chester Tester";

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, fullNameValue), new Claim("name", fullNameValue) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Act
        var fullName = claimsPrincipal.GetFullName();

        // Assert
        fullName.Should().Be(fullNameValue);
    }

    [Test]
    public void GetFullName_Returns_Unknown_User_When_No_Name_Claim()
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Act
        var fullName = claimsPrincipal.GetFullName();

        // Assert
        fullName.Should().Be("Unknown user");
    }

    [Test]
    public void GetUsername_Returns_Username_Claim()
    {
        //Arrange
        var username = "name@email.com";
        var claims = new List<Claim> { new Claim("signInNames.emailAddress", username), new Claim("username", username), new Claim(ClaimTypes.Email, username) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Act
        var result = claimsPrincipal.GetUsername();

        // Assert
        result.Should().Be(username);
    }

    [Test]
    public void GetUsername_Returns_UnknownUser_WhenNoUsernameClaim()
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Act
        var result = claimsPrincipal.GetUsername();

        // Assert
        result.Should().Be("Unknown user");
    }

    [Test]
    public void GetUserId_Returns_UserIdentifier_Claim()
    {
        // Arrange
        var nameIdentifierValue = Guid.NewGuid();

        var claims = new List<Claim> { new Claim(ClaimConstants.NameIdentifierId, nameIdentifierValue.ToString()) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Act
        var userId = claimsPrincipal.GetUserId();

        // Assert
        userId.Should().Be(nameIdentifierValue);
    }

    [Test]
    public void GetUserId_Returns_Empty_Guid_When_No_UserIdentifier_Claim()
    {
        // Arrange
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Act
        var userId = claimsPrincipal.GetUserId();

        // Assert
        userId.Should().BeEmpty();
    }
}
