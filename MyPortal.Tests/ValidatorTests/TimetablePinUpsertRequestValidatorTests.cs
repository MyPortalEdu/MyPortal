using MyPortal.Contracts.Models.Timetabler;
using MyPortal.Services.Validation.Timetabler;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class TimetablePinUpsertRequestValidatorTests
{
    private TimetablePinUpsertRequestValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new TimetablePinUpsertRequestValidator();

    private static TimetablePinUpsertRequest MakeBlockLevelPin() => new()
    {
        CurriculumBlockId = Guid.NewGuid(),
        SlotIndex = 0,
        StartAttendancePeriodId = Guid.NewGuid(),
    };

    private static TimetablePinUpsertRequest MakeClassPin(bool teacher = true, bool room = false) => new()
    {
        CurriculumBlockId = Guid.NewGuid(),
        SlotIndex = 0,
        ClassId = Guid.NewGuid(),
        TeacherId = teacher ? Guid.NewGuid() : null,
        RoomId = room ? Guid.NewGuid() : null,
    };

    [Test]
    public void Validate_AcceptsBlockLevelStartPeriodPin()
    {
        var result = _validator.Validate(MakeBlockLevelPin());
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_AcceptsClassTeacherPin()
    {
        var result = _validator.Validate(MakeClassPin(teacher: true));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_AcceptsClassRoomPin()
    {
        var result = _validator.Validate(MakeClassPin(teacher: false, room: true));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyCurriculumBlockId()
    {
        var pin = MakeBlockLevelPin();
        pin.CurriculumBlockId = Guid.Empty;

        var result = _validator.Validate(pin);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property(nameof(FluentValidation.Results.ValidationFailure.PropertyName))
            .EqualTo(nameof(TimetablePinUpsertRequest.CurriculumBlockId)));
    }

    [Test]
    public void Validate_RejectsPinWithNothingFixed()
    {
        // No teacher, no room, no period — the pin would be a no-op, refuse the request.
        var pin = new TimetablePinUpsertRequest
        {
            CurriculumBlockId = Guid.NewGuid(),
            SlotIndex = 0,
        };

        var result = _validator.Validate(pin);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_RejectsTeacherPinWithoutClassId()
    {
        // Teacher is per-(slot, class) — a pin specifying only TeacherId has no class to attach
        // to and the solver wouldn't know what to do with it.
        var pin = new TimetablePinUpsertRequest
        {
            CurriculumBlockId = Guid.NewGuid(),
            SlotIndex = 0,
            TeacherId = Guid.NewGuid(),
        };

        var result = _validator.Validate(pin);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_RejectsRoomPinWithoutClassId()
    {
        var pin = new TimetablePinUpsertRequest
        {
            CurriculumBlockId = Guid.NewGuid(),
            SlotIndex = 0,
            RoomId = Guid.NewGuid(),
        };

        var result = _validator.Validate(pin);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_AcceptsCombinedTeacherAndRoomPin()
    {
        // Pinning all three (teacher, room, start period) for one class is fine.
        var pin = new TimetablePinUpsertRequest
        {
            CurriculumBlockId = Guid.NewGuid(),
            SlotIndex = 0,
            ClassId = Guid.NewGuid(),
            TeacherId = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            StartAttendancePeriodId = Guid.NewGuid(),
        };

        var result = _validator.Validate(pin);

        Assert.That(result.IsValid, Is.True);
    }
}
