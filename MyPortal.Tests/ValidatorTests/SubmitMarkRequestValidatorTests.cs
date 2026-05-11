using MyPortal.Contracts.Models.Attendance;
using MyPortal.Services.Validation.Attendance;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class SubmitMarkRequestValidatorTests
{
    private SubmitMarkRequestValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new SubmitMarkRequestValidator();
    }

    private static SubmitMarkRequest ValidMark() => new()
    {
        StudentId = Guid.NewGuid(),
        AttendanceCodeId = Guid.NewGuid()
    };

    [Test]
    public void Validate_AcceptsMinimalValidMark()
    {
        var result = _validator.Validate(ValidMark());

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyStudentId()
    {
        var mark = ValidMark();
        mark.StudentId = Guid.Empty;

        var result = _validator.Validate(mark);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property(nameof(FluentValidation.Results.ValidationFailure.PropertyName))
            .EqualTo(nameof(SubmitMarkRequest.StudentId)));
    }

    [Test]
    public void Validate_RejectsEmptyAttendanceCodeId()
    {
        // Marks must always have a code — null/empty is rejected so a register submission can't
        // create marks with no associated AttendanceCode.
        var mark = ValidMark();
        mark.AttendanceCodeId = Guid.Empty;

        var result = _validator.Validate(mark);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property(nameof(FluentValidation.Results.ValidationFailure.PropertyName))
            .EqualTo(nameof(SubmitMarkRequest.AttendanceCodeId)));
    }

    [Test]
    public void Validate_AcceptsCommentsAtMaxLength()
    {
        var mark = ValidMark();
        mark.Comments = new string('x', 256);

        var result = _validator.Validate(mark);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_RejectsCommentsLongerThanMaxLength()
    {
        var mark = ValidMark();
        mark.Comments = new string('x', 257);

        var result = _validator.Validate(mark);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property(nameof(FluentValidation.Results.ValidationFailure.PropertyName))
            .EqualTo(nameof(SubmitMarkRequest.Comments)));
    }

    [Test]
    public void Validate_AcceptsNullComments()
    {
        var mark = ValidMark();
        mark.Comments = null;

        var result = _validator.Validate(mark);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_AcceptsNullMinutesLate()
    {
        var mark = ValidMark();
        mark.MinutesLate = null;

        var result = _validator.Validate(mark);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_AcceptsPositiveMinutesLate()
    {
        var mark = ValidMark();
        mark.MinutesLate = 5;

        var result = _validator.Validate(mark);

        Assert.That(result.IsValid, Is.True);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_RejectsNonPositiveMinutesLate(int minutesLate)
    {
        var mark = ValidMark();
        mark.MinutesLate = minutesLate;

        var result = _validator.Validate(mark);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property(nameof(FluentValidation.Results.ValidationFailure.PropertyName))
            .EqualTo(nameof(SubmitMarkRequest.MinutesLate)));
    }
}
