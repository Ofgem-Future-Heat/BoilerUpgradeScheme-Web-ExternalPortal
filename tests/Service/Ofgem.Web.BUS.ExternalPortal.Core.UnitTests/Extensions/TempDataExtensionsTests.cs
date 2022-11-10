using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;
using System;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Extensions;

[TestFixture]
public class TempDataExtensionsTests
{
    private ITempDataDictionary _tempData;

    [SetUp]
    public void TestFixtureSetup()
    {
        var tempDataProvider = Mock.Of<ITempDataProvider>();
        var factory = new TempDataDictionaryFactory(tempDataProvider);
        _tempData = factory.GetTempData(new DefaultHttpContext());
    }

    [Test]
    public void Put_Adds_Simple_Value_To_TempData()
    {
        // Arrange
        var key = "myKey";
        var value = "Hello, world";
        var serializedValue = JsonConvert.SerializeObject(value);

        // Act
        _tempData.Put(key, value);

        // Assert
        _tempData[key].Should().NotBeNull().And.Be(serializedValue);
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
        _tempData.Put(key, value);

        // Assert
        _tempData[key].Should().NotBeNull().And.Be(serializedValue);
    }

    [Test]
    public void Get_Returns_Null_For_Non_Existent_Key()
    {
        // Arrange
        var key = "myKey";

        // Act
        var value = _tempData.Get<string>(key);

        // Assert
        value.Should().BeNull();
    }

    [Test]
    public void Get_Returns_Simple_Value()
    {
        // Arrange
        var key = "myKey";
        var value = "Hello, world";
        _tempData.Put(key, value);

        // Act
        var retrievedValue = _tempData.Get<string>(key);

        // Assert
        retrievedValue.Should().NotBeNullOrEmpty().And.Be(value);
    }

    [Test]
    public void Get_Returns_Complex_Value()
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
        _tempData.Put(key, value);

        // Act
        var retrievedValue = _tempData.Get<CreateApplicationRequest>(key);

        // Assert
        retrievedValue.Should().NotBeNull().And.BeEquivalentTo(value);
    }
}
