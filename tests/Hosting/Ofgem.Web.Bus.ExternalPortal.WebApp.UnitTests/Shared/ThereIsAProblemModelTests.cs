using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.BaseSetup;
using Ofgem.Web.BUS.ExternalPortal.WebApp.Pages.Shared;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Shared
{
    [TestFixture]
    public class ThereIsAProblemModelTests : PageModelTestsBase
    {
        private ThereIsAProblemModel _systemUnderTest;
        protected Mock<ILogger<ThereIsAProblemModel>> _mockLogger = new();

        [SetUp]
        public override void TestCaseSetUp()
        {
            _systemUnderTest = new ThereIsAProblemModel(_mockLogger.Object);
            base.TestCaseSetUp();
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentNullException_If_Logger_Null()
        {
            // Arrange and Act.
            var action = () => new ThereIsAProblemModel(null!);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Test]
        public void OnGet_Returns_Page()
        {
            // Arrange
            _systemUnderTest = new ThereIsAProblemModel(_mockLogger.Object);

            // Act
            var result = _systemUnderTest.OnGet();

            // Assert
            result.Should().BeOfType<PageResult>();
        }
    }
}