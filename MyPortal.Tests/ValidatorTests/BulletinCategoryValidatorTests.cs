using FluentValidation.Results;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Services.Validation.School;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class BulletinCategoryValidatorTests
{
    private BulletinCategoryValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new BulletinCategoryValidator();
    }

    private static BulletinCategoryUpsertRequest ValidCategory() => new()
    {
        Name = "Notices",
        Icon = "pi pi-megaphone",
        ColourCode = "#6366F1",
        DisplayOrder = 1,
        Active = true
    };

    private static bool HasErrorFor(ValidationResult result, string propertyName) =>
        result.Errors.Any(e => e.PropertyName == propertyName);

    [Test]
    public void Validate_AcceptsMinimalValidCategory()
    {
        var result = _validator.Validate(ValidCategory());

        Assert.That(result.IsValid, Is.True, () => string.Join("; ", result.Errors));
    }

    [Test]
    public void Validate_RejectsEmptyName()
    {
        var m = ValidCategory();
        m.Name = "";

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinCategoryUpsertRequest.Name)), Is.True);
    }

    [Test]
    public void Validate_RejectsNameOver50Chars()
    {
        var m = ValidCategory();
        m.Name = new string('x', 51);

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinCategoryUpsertRequest.Name)), Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyIcon()
    {
        var m = ValidCategory();
        m.Icon = "";

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinCategoryUpsertRequest.Icon)), Is.True);
    }

    [Test]
    public void Validate_RejectsIconOver50Chars()
    {
        var m = ValidCategory();
        m.Icon = new string('x', 51);

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinCategoryUpsertRequest.Icon)), Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyColourCode()
    {
        var m = ValidCategory();
        m.ColourCode = "";

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinCategoryUpsertRequest.ColourCode)), Is.True);
    }

    [TestCase("#6366F1")]      // 6-digit RGB
    [TestCase("#6366f1")]      // lower-case
    [TestCase("#6366F1FF")]    // 8-digit RGBA (the case the FE tint() bug fix relies on)
    [TestCase("#6366f1ff")]    // 8-digit lower-case
    public void Validate_AcceptsHex_6Or8Digit(string colour)
    {
        var m = ValidCategory();
        m.ColourCode = colour;

        var result = _validator.Validate(m);

        Assert.That(result.IsValid, Is.True, () => string.Join("; ", result.Errors));
    }

    [TestCase("6366F1")]       // missing hash
    [TestCase("#6366F")]       // 5 digits
    [TestCase("#6366F1F")]     // 7 digits
    [TestCase("#6366F1FFF")]   // 9 digits — over the 9-char cap AND wrong length
    [TestCase("#GGGGGG")]      // non-hex chars
    [TestCase("red")]          // CSS keyword
    public void Validate_RejectsMalformedColour(string colour)
    {
        var m = ValidCategory();
        m.ColourCode = colour;

        var result = _validator.Validate(m);

        Assert.That(result.IsValid, Is.False);
        Assert.That(HasErrorFor(result, nameof(BulletinCategoryUpsertRequest.ColourCode)), Is.True);
    }
}
