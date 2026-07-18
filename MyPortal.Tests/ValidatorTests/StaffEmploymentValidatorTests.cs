using MyPortal.Contracts.Models.People;
using MyPortal.Services.Validation.People;

namespace MyPortal.Tests.ValidatorTests;

[TestFixture]
public class StaffEmploymentValidatorTests
{
    private StaffEmploymentDetailsUpsertRequestValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new StaffEmploymentDetailsUpsertRequestValidator();

    private static StaffEmploymentUpsertItem Emp(DateTime start, DateTime? end,
        params StaffContractUpsertItem[] contracts) => new()
    {
        StartDate = start,
        EndDate = end,
        Contracts = contracts.ToList()
    };

    private static StaffContractUpsertItem Contract(DateTime start, DateTime? end) => new()
    {
        ContractTypeId = Guid.NewGuid(),
        PostTitle = "Teacher",
        StartDate = start,
        EndDate = end,
        Fte = 1m
    };

    private static StaffEmploymentDetailsUpsertRequest Request(params StaffEmploymentUpsertItem[] employments) =>
        new() { Employments = employments.ToList() };

    private bool HasError(StaffEmploymentDetailsUpsertRequest m, string propertyName) =>
        _validator.Validate(m).Errors.Any(e => e.PropertyName == propertyName);

    private bool HasMessage(StaffEmploymentDetailsUpsertRequest m, string fragment) =>
        _validator.Validate(m).Errors.Any(e => e.ErrorMessage.Contains(fragment));

    [Test]
    public void Validate_RejectsOverlappingEmployments()
    {
        var m = Request(
            Emp(new DateTime(2020, 1, 1), new DateTime(2021, 1, 1)),
            Emp(new DateTime(2020, 6, 1), new DateTime(2022, 1, 1)));

        Assert.That(HasError(m, nameof(StaffEmploymentDetailsUpsertRequest.Employments)), Is.True);
    }

    [Test]
    public void Validate_RejectsTwoOpenEndedEmployments()
    {
        var m = Request(
            Emp(new DateTime(2020, 1, 1), null),
            Emp(new DateTime(2021, 1, 1), null));

        Assert.That(HasError(m, nameof(StaffEmploymentDetailsUpsertRequest.Employments)), Is.True);
    }

    [Test]
    public void Validate_AcceptsSequentialEmployments()
    {
        var m = Request(
            Emp(new DateTime(2020, 1, 1), new DateTime(2021, 1, 1)),
            Emp(new DateTime(2021, 6, 1), null));

        Assert.That(HasError(m, nameof(StaffEmploymentDetailsUpsertRequest.Employments)), Is.False);
    }

    [Test]
    public void Validate_RejectsContractStartingBeforeEmployment()
    {
        var m = Request(
            Emp(new DateTime(2020, 1, 1), new DateTime(2021, 1, 1),
                Contract(new DateTime(2019, 1, 1), new DateTime(2020, 6, 1))));

        Assert.That(HasMessage(m, "within its employment period"), Is.True);
    }

    [Test]
    public void Validate_RejectsContractEndingAfterEmployment()
    {
        var m = Request(
            Emp(new DateTime(2020, 1, 1), new DateTime(2021, 1, 1),
                Contract(new DateTime(2020, 2, 1), new DateTime(2021, 6, 1))));

        Assert.That(HasMessage(m, "within its employment period"), Is.True);
    }

    [Test]
    public void Validate_AcceptsContractWithinEmployment()
    {
        var m = Request(
            Emp(new DateTime(2020, 1, 1), new DateTime(2021, 1, 1),
                Contract(new DateTime(2020, 2, 1), new DateTime(2020, 12, 1))));

        Assert.That(HasMessage(m, "within its employment period"), Is.False);
    }
}
