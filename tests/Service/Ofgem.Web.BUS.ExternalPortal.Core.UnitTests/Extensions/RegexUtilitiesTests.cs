using FluentAssertions;
using NUnit.Framework;
using Ofgem.Web.BUS.ExternalPortal.Core.Utilities;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.Extensions;

[TestFixture]
public class RegexUtilitiesTests
{
    [TestCase("hello@world.com")]
    [TestCase("héllo@wórld.com")]
    [TestCase("joe.bloggs@ofgem.gov.uk")]
    [TestCase("joe_bloggs@ofgem.gov.uk")]
    [TestCase("joe.bloggs+something@ofgem.gov.uk")]
    public void IsValidEmail_Should_Match_Valid_Email(string email)
    {
        // Act
        var isValidEmailResult = RegexUtilities.IsValidEmail(email);

        // Assert
        isValidEmailResult.Should().BeTrue();
    }

    [TestCase("")]
    [TestCase("This is a sentence")]
    [TestCase("joe.bloggs'ofgem.gov.uk")]
    [TestCase("joe.bloggs@ofgem@ofgem.gov.uk")]
    [TestCase("joe.bloggs@of gem.gov.uk")]
    public void IsValidEmail_Should_Not_Match_Invalid_Email(string email)
    {
        // Act
        var isValidEmailResult = RegexUtilities.IsValidEmail(email);

        // Assert
        isValidEmailResult.Should().BeFalse();
    }

    [TestCase("AB12 3CD", "AB12 3CD")]
    [TestCase("  AB12  3CD  ", "AB12 3CD")]
    [TestCase("ab12 3cd", "AB12 3CD")]
    [TestCase("AB12-3CD!!", "AB123CD")]
    [TestCase("A12 3CD", "A12 3CD")]
    [TestCase("A12B 3CD", "A12B 3CD")]
    [TestCase("", "")]
    [TestCase(" ", " ")]
    public void ParsePostcode_Writes_Postcode_In_Correct_Format(string input, string expected)
    {
        // Act
        var parsePostcodeResult = RegexUtilities.ParsePostcode(input);

        // Act
        parsePostcodeResult.Should().Be(expected);
    }

    [TestCase("AB12 3CD", "AB12 3CD")]
    [TestCase("  AB12  3CD  ", "AB12 3CD")]
    [TestCase("  AB123CD  ", "AB12 3CD")]
    [TestCase("ab12 3cd", "AB12 3CD")]
    [TestCase("AB12-3CD!!", "AB12 3CD")]
    [TestCase("A12 3CD", "A12 3CD")]
    [TestCase("A12B 3CD", "A12B 3CD")]
    [TestCase("A12B3CD", "A12B 3CD")]
    [TestCase("B13CD", "B1 3CD")]
    [TestCase("B113CD", "B11 3CD")]
    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase(" ", " ")]
    public void NormalisePostcode_Writes_Postcode_In_Correct_Format(string input, string expected)
    {
        // Act
        var parsePostcodeResult = RegexUtilities.NormalisePostcode(input);

        // Act
        parsePostcodeResult.Should().Be(expected);
    }
}
