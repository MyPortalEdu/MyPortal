using MyPortal.Contracts.Models.Attendance;
using MyPortal.Services.Validation.Attendance;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class SubmitRegisterRequestValidatorTests
{
    private SubmitRegisterRequestValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new SubmitRegisterRequestValidator();
    }

    private static SubmitMarkRequest MarkFor(Guid studentId) => new()
    {
        StudentId = studentId,
        AttendanceCodeId = Guid.NewGuid()
    };

    [Test]
    public void Validate_AcceptsValidMarks()
    {
        var request = new SubmitRegisterRequest
        {
            Marks = new List<SubmitMarkRequest> { MarkFor(Guid.NewGuid()), MarkFor(Guid.NewGuid()) }
        };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_AcceptsEmptyMarks()
    {
        // Open-and-immediately-close behaviour: a teacher hitting "save" on a fresh register
        // before marking anyone must not be rejected by validation.
        var request = new SubmitRegisterRequest { Marks = new List<SubmitMarkRequest>() };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_RejectsNullMarks()
    {
        var request = new SubmitRegisterRequest { Marks = null! };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property(nameof(FluentValidation.Results.ValidationFailure.PropertyName))
            .EqualTo(nameof(SubmitRegisterRequest.Marks)));
    }

    [Test]
    public void Validate_RejectsDuplicateStudentIds()
    {
        // Each student gets exactly one mark per (period, week); two marks for the same student
        // in one submission is a client bug and would race the upsert.
        var sharedStudent = Guid.NewGuid();
        var request = new SubmitRegisterRequest
        {
            Marks = new List<SubmitMarkRequest> { MarkFor(sharedStudent), MarkFor(sharedStudent) }
        };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property(nameof(FluentValidation.Results.ValidationFailure.PropertyName))
            .EqualTo(nameof(SubmitRegisterRequest.Marks)));
    }

    [Test]
    public void Validate_AggregatesPerMarkErrors()
    {
        // Per-mark rules (delegated to SubmitMarkRequestValidator) must surface up through the
        // composite, so a bad mark within a set fails the whole request.
        var request = new SubmitRegisterRequest
        {
            Marks = new List<SubmitMarkRequest>
            {
                MarkFor(Guid.NewGuid()),
                new() { StudentId = Guid.NewGuid(), AttendanceCodeId = Guid.Empty }
            }
        };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property(nameof(FluentValidation.Results.ValidationFailure.PropertyName))
            .Contains(nameof(SubmitMarkRequest.AttendanceCodeId)));
    }
}
