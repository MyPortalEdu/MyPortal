using FluentValidation.Results;
using Moq;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Services.Interfaces.Providers;
using MyPortal.Services.Validation.School;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class BulletinValidatorTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private BulletinValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        var clock = new Mock<IDateTimeProvider>(MockBehavior.Strict);
        clock.SetupGet(c => c.UtcNow).Returns(FixedNow);
        _validator = new BulletinValidator(clock.Object);
    }

    private static BulletinUpsertRequest ValidBulletin() => new()
    {
        Title = "Title",
        Detail = "Detail",
        CategoryId = Guid.NewGuid(),
        ExpiresAt = null,
        Audiences = new List<BulletinAudienceRequest>
        {
            new() { AudienceKind = BulletinAudienceKind.AllStaff }
        }
    };

    private static bool HasErrorFor(ValidationResult result, string propertyName) =>
        result.Errors.Any(e => e.PropertyName == propertyName || e.PropertyName.StartsWith(propertyName + "[", StringComparison.Ordinal));

    [Test]
    public void Validate_AcceptsMinimalValidBulletin()
    {
        var result = _validator.Validate(ValidBulletin());

        Assert.That(result.IsValid, Is.True, () => string.Join("; ", result.Errors));
    }

    [Test]
    public void Validate_RejectsEmptyTitle()
    {
        var m = ValidBulletin();
        m.Title = "";

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.Title)), Is.True);
    }

    [Test]
    public void Validate_RejectsTitleOver50Chars()
    {
        var m = ValidBulletin();
        m.Title = new string('x', 51);

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.Title)), Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyDetail()
    {
        var m = ValidBulletin();
        m.Detail = "";

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.Detail)), Is.True);
    }

    [Test]
    public void Validate_RejectsDetailOver2000Chars()
    {
        var m = ValidBulletin();
        m.Detail = new string('x', 2001);

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.Detail)), Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyCategoryId()
    {
        var m = ValidBulletin();
        m.CategoryId = Guid.Empty;

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.CategoryId)), Is.True);
    }

    [Test]
    public void Validate_AllowsNullExpiry()
    {
        var m = ValidBulletin();
        m.ExpiresAt = null;

        var result = _validator.Validate(m);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_RejectsExpiryInThePast()
    {
        var m = ValidBulletin();
        m.ExpiresAt = FixedNow.AddMinutes(-1);

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.ExpiresAt)), Is.True);
    }

    [Test]
    public void Validate_RejectsExpiryEqualToNow()
    {
        // Rule is strict-greater-than: an expiry equal to "now" is in the past by the
        // time the bulletin is saved, so reject.
        var m = ValidBulletin();
        m.ExpiresAt = FixedNow;

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.ExpiresAt)), Is.True);
    }

    [Test]
    public void Validate_AcceptsExpiryInTheFuture()
    {
        var m = ValidBulletin();
        m.ExpiresAt = FixedNow.AddDays(7);

        var result = _validator.Validate(m);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyAudienceList()
    {
        var m = ValidBulletin();
        m.Audiences.Clear();

        var result = _validator.Validate(m);

        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.Audiences)), Is.True);
    }

    [Test]
    public void Validate_RejectsStudentGroupAudienceWithoutGroupId()
    {
        var m = ValidBulletin();
        m.Audiences = new List<BulletinAudienceRequest>
        {
            new() { AudienceKind = BulletinAudienceKind.StudentGroup, StudentGroupId = null }
        };

        var result = _validator.Validate(m);

        Assert.That(result.IsValid, Is.False);
        Assert.That(HasErrorFor(result, nameof(BulletinUpsertRequest.Audiences)), Is.True);
    }

    [Test]
    public void Validate_RejectsAllStaffAudienceWithGroupId()
    {
        // StudentGroupId is meaningful only when AudienceKind = StudentGroup; otherwise
        // it's malformed and would silently confuse the audience-matching SP.
        var m = ValidBulletin();
        m.Audiences = new List<BulletinAudienceRequest>
        {
            new() { AudienceKind = BulletinAudienceKind.AllStaff, StudentGroupId = Guid.NewGuid() }
        };

        var result = _validator.Validate(m);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_AcceptsStudentGroupAudienceWithGroupId()
    {
        var m = ValidBulletin();
        m.Audiences = new List<BulletinAudienceRequest>
        {
            new() { AudienceKind = BulletinAudienceKind.StudentGroup, StudentGroupId = Guid.NewGuid() }
        };

        var result = _validator.Validate(m);

        Assert.That(result.IsValid, Is.True);
    }
}
