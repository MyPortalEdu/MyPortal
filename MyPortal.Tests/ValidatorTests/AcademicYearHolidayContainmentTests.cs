using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Curriculum;
using MyPortal.Services.Validation.Curriculum;

namespace MyPortal.Tests.ValidatorTests;

/// <summary>
/// Focused on the holiday-within-term-span rule. The full request has many other rules; these
/// tests assert only whether the "SchoolHolidays" containment error is raised.
/// </summary>
[TestFixture]
public class AcademicYearHolidayContainmentTests
{
    private AcademicYearUpsertRequestValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new AcademicYearUpsertRequestValidator();

    private static AcademicTermUpsertRequest[] SingleTerm() =>
    [
        new() { Name = "Full Year", StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2027, 7, 20) }
    ];

    private bool HasHolidayContainmentError(AcademicYearUpsertRequest model) =>
        _validator.Validate(model).Errors.Any(e =>
            e.PropertyName == nameof(AcademicYearUpsertRequest.SchoolHolidays) &&
            e.ErrorMessage.Contains("within the academic year"));

    private static AcademicYearUpsertRequest WithHoliday(DateTime start, DateTime end) => new()
    {
        AcademicTerms = SingleTerm(),
        SchoolHolidays =
        [
            new() { Name = "Break", Type = SchoolHolidayType.HalfTerm, StartDate = start, EndDate = end }
        ]
    };

    [Test]
    public void Validate_AcceptsHolidayWithinTermSpan() =>
        Assert.That(HasHolidayContainmentError(WithHoliday(new DateTime(2026, 12, 21), new DateTime(2027, 1, 2))),
            Is.False);

    [Test]
    public void Validate_RejectsHolidayEndingAfterLastTermEnds() =>
        Assert.That(HasHolidayContainmentError(WithHoliday(new DateTime(2027, 8, 1), new DateTime(2027, 8, 30))),
            Is.True);

    [Test]
    public void Validate_RejectsHolidayStartingBeforeFirstTermStarts() =>
        Assert.That(HasHolidayContainmentError(WithHoliday(new DateTime(2026, 8, 20), new DateTime(2026, 9, 5))),
            Is.True);
}
