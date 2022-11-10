using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Fakes;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using System;
using System.Text;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Extensions;

[TestFixture]
public class SessionExtensionsTests
{
    private FakeHttpSession _session = new FakeHttpSession();

    [SetUp]
    public void TestCaseSetUp()
    {
        _session = new FakeHttpSession();
    }

    [Test]
    public void Put_Adds_Simple_Value_To_Session()
    {
        // Arrange
        var key = "myKey";
        var value = "Hello, world";
        var serializedValue = JsonConvert.SerializeObject(value);

        // Act
        _session.Put(key, value);

        // Assert
        var rawValue = _session.GetString(key);
        rawValue.Should().NotBeNull().And.Be(serializedValue);
    }

    [Test]
    public void Put_Adds_Complex_Value_To_TempData()
    {
        // Arrange
        var key = "myKey";
        var value = new CreateApplicationRequest
        {
            ApplicationDate = DateTime.Now,
            QuoteAmount = 9000,
            InstallationAddress = new CreateApplicationRequestInstallationAddress
            {
                Line1 = "line1",
                Line2 = "line2"
            }
        };
        var serializedValue = JsonConvert.SerializeObject(value);

        // Act
        _session.Put(key, value);

        // Assert
        var rawValue = _session.GetString(key);
        rawValue.Should().NotBeNull().And.Be(serializedValue);
    }

    [Test]
    public void GetOrDefault_Returns_Null_For_Non_Existent_Key()
    {
        // Arrange
        var key = "myKey";

        // Act
        var value = _session.GetOrDefault<string>(key);

        // Assert
        value.Should().BeNull();
    }

    [Test]
    public void GetOrDefault_Returns_Simple_Value()
    {
        // Arrange
        var key = "myKey";
        var value = "Hello, world";
        _session.Put(key, value);

        // Act
        var retrievedValue = _session.GetOrDefault<string>(key);

        // Assert
        retrievedValue.Should().NotBeNullOrEmpty().And.Be(value);
    }

    [Test]
    public void GetOrDefault_Returns_Complex_Value()
    {
        // Arrange
        var key = "myKey";
        var value = new CreateApplicationRequest
        {
            ApplicationDate = DateTime.Now,
            QuoteAmount = 9000,
            InstallationAddress = new CreateApplicationRequestInstallationAddress
            {
                Line1 = "line1",
                Line2 = "line2"
            }
        };
        _session.Put(key, value);

        // Act
        var retrievedValue = _session.GetOrDefault<CreateApplicationRequest>(key);

        // Assert
        retrievedValue.Should().NotBeNull().And.BeEquivalentTo(value);
    }
}
