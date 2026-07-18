using FluentValidation.Results;
using MyPortal.Contracts.Models.School;
using MyPortal.Services.Validation.School;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class SchoolValidatorTests
{
    private SchoolValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new SchoolValidator();

    private static SchoolUpsertRequest ValidSchool() => new()
    {
        Name = "Test School",
        Urn = "123456",
        Uprn = "100000000001",
        EstablishmentNumber = 1234,
        SchoolPhaseId = Guid.NewGuid(),
        SchoolTypeId = Guid.NewGuid(),
        GovernanceTypeId = Guid.NewGuid(),
        IntakeTypeId = Guid.NewGuid(),
        IsSpecialSchool = false
    };

    private static bool HasErrorFor(ValidationResult result, string propertyName) =>
        result.Errors.Any(e => e.PropertyName == propertyName);

    [Test]
    public void Validate_AcceptsMinimalValidSchool()
    {
        var result = _validator.Validate(ValidSchool());
        Assert.That(result.IsValid, Is.True, () => string.Join("; ", result.Errors));
    }

    [Test]
    public void Validate_RejectsEmptyName()
    {
        var m = ValidSchool();
        m.Name = "";
        Assert.That(HasErrorFor(_validator.Validate(m), nameof(SchoolUpsertRequest.Name)), Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyUrn()
    {
        var m = ValidSchool();
        m.Urn = "";
        Assert.That(HasErrorFor(_validator.Validate(m), nameof(SchoolUpsertRequest.Urn)), Is.True);
    }

    [Test]
    public void Validate_RejectsMissingRequiredLookup()
    {
        var m = ValidSchool();
        m.SchoolPhaseId = Guid.Empty;
        Assert.That(HasErrorFor(_validator.Validate(m), nameof(SchoolUpsertRequest.SchoolPhaseId)), Is.True);
    }

    [Test]
    public void Validate_RejectsNegativeEstablishmentNumber()
    {
        var m = ValidSchool();
        m.EstablishmentNumber = -1;
        Assert.That(HasErrorFor(_validator.Validate(m), nameof(SchoolUpsertRequest.EstablishmentNumber)), Is.True);
    }

    [TestCase("1234567")]   // 7 digits
    [TestCase("123456789")] // 9 digits
    [TestCase("1234567a")]  // non-numeric
    public void Validate_RejectsMalformedUkprn(string ukprn)
    {
        var m = ValidSchool();
        m.Ukprn = ukprn;
        Assert.That(HasErrorFor(_validator.Validate(m), nameof(SchoolUpsertRequest.Ukprn)), Is.True);
    }

    [Test]
    public void Validate_AcceptsEightDigitUkprn()
    {
        var m = ValidSchool();
        m.Ukprn = "10000001";
        Assert.That(_validator.Validate(m).IsValid, Is.True);
    }

    [Test]
    public void Validate_RejectsHighestAgeBelowLowestAge()
    {
        var m = ValidSchool();
        m.LowestAge = 11;
        m.HighestAge = 4;
        Assert.That(HasErrorFor(_validator.Validate(m), nameof(SchoolUpsertRequest.HighestAge)), Is.True);
    }

    [Test]
    public void Validate_AcceptsOrderedAgeRange()
    {
        var m = ValidSchool();
        m.LowestAge = 4;
        m.HighestAge = 11;
        Assert.That(_validator.Validate(m).IsValid, Is.True);
    }

    [Test]
    public void Validate_RequiresSpecialSchoolFields_WhenSpecial()
    {
        var m = ValidSchool();
        m.IsSpecialSchool = true;
        var result = _validator.Validate(m);
        Assert.That(HasErrorFor(result, nameof(SchoolUpsertRequest.SpecialSchoolOrganisationId)), Is.True);
        Assert.That(HasErrorFor(result, nameof(SchoolUpsertRequest.SpecialSchoolTypeId)), Is.True);
    }

    [Test]
    public void Validate_AcceptsSpecialSchool_WithFields()
    {
        var m = ValidSchool();
        m.IsSpecialSchool = true;
        m.SpecialSchoolOrganisationId = Guid.NewGuid();
        m.SpecialSchoolTypeId = Guid.NewGuid();
        Assert.That(_validator.Validate(m).IsValid, Is.True, () => string.Join("; ", _validator.Validate(m).Errors));
    }

    [Test]
    public void Validate_RejectsMalformedEmail()
    {
        var m = ValidSchool();
        m.Email = "not-an-email";
        Assert.That(HasErrorFor(_validator.Validate(m), nameof(SchoolUpsertRequest.Email)), Is.True);
    }
}
