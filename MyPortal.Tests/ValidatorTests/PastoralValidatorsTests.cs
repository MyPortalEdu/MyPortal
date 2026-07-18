using FluentValidation.Results;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Services.Validation.Pastoral;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class PastoralValidatorsTests
{
    private readonly HouseUpsertRequestValidator _house = new();
    private readonly YearGroupUpsertRequestValidator _yearGroup = new();
    private readonly RegGroupUpsertRequestValidator _regGroup = new();
    private readonly StudentGroupSupervisorUpsertRequestValidator _supervisor = new();

    private static bool HasErrorFor(ValidationResult result, string propertyName) =>
        result.Errors.Any(e => e.PropertyName == propertyName);

    private static StudentGroupSupervisorUpsertRequest ValidSupervisor() => new()
    {
        StaffMemberId = Guid.NewGuid(),
        Title = "Form Tutor",
        MainSupervisor = true
    };

    private static HouseUpsertRequest ValidHouse() => new()
    {
        AcademicYearId = Guid.NewGuid(),
        Code = "RED",
        Name = "Red House",
        ColourCode = "#FF0000",
        Active = true
    };

    private static YearGroupUpsertRequest ValidYearGroup() => new()
    {
        AcademicYearId = Guid.NewGuid(),
        CurriculumYearGroupId = Guid.NewGuid(),
        Code = "Y7",
        Name = "Year 7",
        Active = true
    };

    private static RegGroupUpsertRequest ValidRegGroup() => new()
    {
        AcademicYearId = Guid.NewGuid(),
        YearGroupId = Guid.NewGuid(),
        Code = "7A",
        Name = "7A",
        Active = true
    };

    // --- Supervisor ---

    [Test]
    public void Supervisor_AcceptsValid() =>
        Assert.That(_supervisor.Validate(ValidSupervisor()).IsValid, Is.True);

    [Test]
    public void Supervisor_RejectsEmptyStaffMember()
    {
        var m = ValidSupervisor();
        m.StaffMemberId = Guid.Empty;
        Assert.That(HasErrorFor(_supervisor.Validate(m), nameof(StudentGroupSupervisorUpsertRequest.StaffMemberId)), Is.True);
    }

    [Test]
    public void Supervisor_RejectsEmptyTitle()
    {
        var m = ValidSupervisor();
        m.Title = "";
        Assert.That(HasErrorFor(_supervisor.Validate(m), nameof(StudentGroupSupervisorUpsertRequest.Title)), Is.True);
    }

    // --- House ---

    [Test]
    public void House_AcceptsValid() =>
        Assert.That(_house.Validate(ValidHouse()).IsValid, Is.True, () => string.Join("; ", _house.Validate(ValidHouse()).Errors));

    [Test]
    public void House_RejectsEmptyCode()
    {
        var m = ValidHouse();
        m.Code = "";
        Assert.That(HasErrorFor(_house.Validate(m), nameof(HouseUpsertRequest.Code)), Is.True);
    }

    [Test]
    public void House_RejectsCodeOver10Chars()
    {
        var m = ValidHouse();
        m.Code = new string('x', 11);
        Assert.That(HasErrorFor(_house.Validate(m), nameof(HouseUpsertRequest.Code)), Is.True);
    }

    [Test]
    public void House_RejectsColourCodeOver10Chars()
    {
        var m = ValidHouse();
        m.ColourCode = new string('x', 11);
        Assert.That(HasErrorFor(_house.Validate(m), nameof(HouseUpsertRequest.ColourCode)), Is.True);
    }

    [Test]
    public void House_RejectsInvalidSupervisorInList()
    {
        var m = ValidHouse();
        m.Supervisors.Add(new StudentGroupSupervisorUpsertRequest { StaffMemberId = Guid.Empty, Title = "" });
        Assert.That(_house.Validate(m).IsValid, Is.False);
    }

    // --- Year group ---

    [Test]
    public void YearGroup_AcceptsValid() =>
        Assert.That(_yearGroup.Validate(ValidYearGroup()).IsValid, Is.True);

    [Test]
    public void YearGroup_RejectsEmptyCurriculumYearGroup()
    {
        var m = ValidYearGroup();
        m.CurriculumYearGroupId = Guid.Empty;
        Assert.That(HasErrorFor(_yearGroup.Validate(m), nameof(YearGroupUpsertRequest.CurriculumYearGroupId)), Is.True);
    }

    // --- Reg group ---

    [Test]
    public void RegGroup_AcceptsValid() =>
        Assert.That(_regGroup.Validate(ValidRegGroup()).IsValid, Is.True);

    [Test]
    public void RegGroup_RejectsEmptyYearGroup()
    {
        var m = ValidRegGroup();
        m.YearGroupId = Guid.Empty;
        Assert.That(HasErrorFor(_regGroup.Validate(m), nameof(RegGroupUpsertRequest.YearGroupId)), Is.True);
    }
}
