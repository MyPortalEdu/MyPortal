using MyPortal.Contracts.Models.People;
using MyPortal.Services.Validation.People;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class StaffAbsenceValidatorTests
{
    private StaffAbsencesUpsertRequestValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new StaffAbsencesUpsertRequestValidator();

    private static StaffAbsenceUpsertItem Absence(DateTime start, DateTime end) => new()
    {
        AbsenceTypeId = Guid.NewGuid(),
        StartDate = start,
        EndDate = end
    };

    private static StaffAbsencesUpsertRequest Request(params StaffAbsenceUpsertItem[] absences) =>
        new() { Absences = absences.ToList() };

    private bool HasAbsencesError(StaffAbsencesUpsertRequest m) =>
        _validator.Validate(m).Errors.Any(e => e.PropertyName == nameof(StaffAbsencesUpsertRequest.Absences));

    [Test]
    public void Validate_RejectsOverlappingAbsences()
    {
        var m = Request(
            Absence(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10)),
            Absence(new DateTime(2020, 1, 5), new DateTime(2020, 1, 15)));

        Assert.That(HasAbsencesError(m), Is.True);
    }

    [Test]
    public void Validate_AcceptsSequentialAbsences()
    {
        var m = Request(
            Absence(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10)),
            Absence(new DateTime(2020, 2, 1), new DateTime(2020, 2, 10)));

        Assert.That(HasAbsencesError(m), Is.False);
    }
}
