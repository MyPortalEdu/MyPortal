using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Attendance;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Attendance;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Attendance;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class AttendanceMarkServiceTests
{
    private Mock<IAuthorizationService> _authorizationService;
    private Mock<ILogger<AttendanceMarkService>> _logger;
    private Mock<IAttendanceMarkRepository> _attendanceMarkRepository;
    private Mock<IAttendanceCodeService> _attendanceCodeService;
    private Mock<IValidationService> _validationService;

    private AttendanceMarkService _service;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<AttendanceMarkService>>(MockBehavior.Loose);
        _attendanceMarkRepository = new Mock<IAttendanceMarkRepository>(MockBehavior.Strict);
        _attendanceCodeService = new Mock<IAttendanceCodeService>(MockBehavior.Strict);
        _validationService = new Mock<IValidationService>(MockBehavior.Strict);

        _service = new AttendanceMarkService(
            _authorizationService.Object,
            _logger.Object,
            _attendanceMarkRepository.Object,
            _attendanceCodeService.Object,
            _validationService.Object);
    }

    private void RequirePermission(string permission, bool succeeds = true)
    {
        var setup = _authorizationService.Setup(a =>
            a.RequirePermissionAsync(permission, It.IsAny<CancellationToken>()));
        if (succeeds)
            setup.Returns(Task.CompletedTask);
        else
            setup.ThrowsAsync(new ForbiddenException($"missing permission: {permission}"));
    }

    private void StubAllCodesValid()
    {
        _attendanceCodeService.Setup(s => s.EnsureCodesAreUsableAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static BulkAttendanceMarksRequest MakeRequest(Guid studentGroupId, DateTime from, DateTime to,
        params BulkAttendanceMarkUpsert[] marks) =>
        new()
        {
            StudentGroupId = studentGroupId,
            From = from,
            To = to,
            Marks = marks.ToList()
        };

    private static BulkAttendanceMarkUpsert Mark(Guid? codeId)
        => new()
        {
            StudentId = Guid.NewGuid(),
            AttendanceWeekId = Guid.NewGuid(),
            AttendancePeriodId = Guid.NewGuid(),
            AttendanceCodeId = codeId
        };

    // ─── GetBulkAsync ────────────────────────────────────────────────────────

    [Test]
    public async Task GetBulkAsync_ReturnsRepoResult_WhenScopeResolves()
    {
        var groupId = Guid.NewGuid();
        var from = new DateTime(2026, 9, 1);
        var to   = new DateTime(2026, 9, 5);
        var dto = new BulkAttendanceMarksResponse { StudentGroupId = groupId, GroupCode = "7L" };

        RequirePermission(Permissions.Attendance.ViewAttendanceMarks);
        _attendanceMarkRepository.Setup(r => r.GetBulkAsync(groupId, from, to,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _service.GetBulkAsync(groupId, from, to, CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void GetBulkAsync_ThrowsNotFound_WhenRepoReturnsNull()
    {
        // Repo returns null when the StudentGroup doesn't exist; map to 404.
        RequirePermission(Permissions.Attendance.ViewAttendanceMarks);
        _attendanceMarkRepository.Setup(r => r.GetBulkAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BulkAttendanceMarksResponse?)null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetBulkAsync(Guid.NewGuid(), new DateTime(2026, 9, 1), new DateTime(2026, 9, 5),
                CancellationToken.None));
    }

    [Test]
    public void GetBulkAsync_RejectsInvertedDateRange()
    {
        // From > To is meaningless; reject before hitting the repo.
        RequirePermission(Permissions.Attendance.ViewAttendanceMarks);

        Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetBulkAsync(Guid.NewGuid(),
                new DateTime(2026, 9, 5), new DateTime(2026, 9, 1), CancellationToken.None));

        _attendanceMarkRepository.Verify(r => r.GetBulkAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void GetBulkAsync_RequiresViewPermission()
    {
        RequirePermission(Permissions.Attendance.ViewAttendanceMarks, succeeds: false);

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.GetBulkAsync(Guid.NewGuid(), new DateTime(2026, 9, 1),
                new DateTime(2026, 9, 5), CancellationToken.None));

        _attendanceMarkRepository.Verify(r => r.GetBulkAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── SubmitBulkAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task SubmitBulkAsync_ForwardsScopeAndMarksToRepo_AfterValidation()
    {
        var groupId = Guid.NewGuid();
        var from = new DateTime(2026, 9, 1);
        var to   = new DateTime(2026, 9, 5);
        var request = MakeRequest(groupId, from, to, Mark(Guid.NewGuid()), Mark(Guid.NewGuid()));

        RequirePermission(Permissions.Attendance.EditAttendanceMarksBulk);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        StubAllCodesValid();
        _attendanceMarkRepository.Setup(r => r.SubmitBulkAsync(groupId, from, to,
                It.IsAny<IReadOnlyCollection<BulkAttendanceMarkUpsert>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SubmitBulkAsync(request, CancellationToken.None);

        _attendanceMarkRepository.Verify(r => r.SubmitBulkAsync(groupId, from, to,
            It.Is<IReadOnlyCollection<BulkAttendanceMarkUpsert>>(m => m.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SubmitBulkAsync_PassesOnlyNonNullCodeIds_ToCodePolicy()
    {
        // Null AttendanceCodeId entries are deletes — the code policy doesn't apply to them
        // (there's no code being used). Capture what we passed and assert the filter happened.
        var groupId = Guid.NewGuid();
        var from = new DateTime(2026, 9, 1);
        var to   = new DateTime(2026, 9, 5);
        var keepCodeId = Guid.NewGuid();
        var request = MakeRequest(groupId, from, to,
            Mark(keepCodeId),       // upsert
            Mark(null),             // delete
            Mark(null));            // delete

        IEnumerable<Guid>? capturedCodeIds = null;
        _attendanceCodeService.Setup(s => s.EnsureCodesAreUsableAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Guid>, CancellationToken>((ids, _) => capturedCodeIds = ids.ToList())
            .Returns(Task.CompletedTask);

        RequirePermission(Permissions.Attendance.EditAttendanceMarksBulk);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        _attendanceMarkRepository.Setup(r => r.SubmitBulkAsync(groupId, from, to,
                It.IsAny<IReadOnlyCollection<BulkAttendanceMarkUpsert>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SubmitBulkAsync(request, CancellationToken.None);

        Assert.That(capturedCodeIds, Is.Not.Null);
        Assert.That(capturedCodeIds!.Single(), Is.EqualTo(keepCodeId));
    }

    [Test]
    public async Task SubmitBulkAsync_AllDeletes_PassesEmptyToCodePolicy()
    {
        // A submission consisting entirely of deletes still flows through the policy (so
        // the no-op path is exercised) but with no codeIds.
        var request = MakeRequest(Guid.NewGuid(), new DateTime(2026, 9, 1), new DateTime(2026, 9, 5),
            Mark(null), Mark(null));

        IEnumerable<Guid>? capturedCodeIds = null;
        _attendanceCodeService.Setup(s => s.EnsureCodesAreUsableAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Guid>, CancellationToken>((ids, _) => capturedCodeIds = ids.ToList())
            .Returns(Task.CompletedTask);

        RequirePermission(Permissions.Attendance.EditAttendanceMarksBulk);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        _attendanceMarkRepository.Setup(r => r.SubmitBulkAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<IReadOnlyCollection<BulkAttendanceMarkUpsert>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SubmitBulkAsync(request, CancellationToken.None);

        Assert.That(capturedCodeIds, Is.Empty);
    }

    [Test]
    public void SubmitBulkAsync_PropagatesValidationFailure_WithoutCallingCodePolicyOrRepo()
    {
        var request = MakeRequest(Guid.NewGuid(), new DateTime(2026, 9, 1), new DateTime(2026, 9, 5),
            Mark(Guid.NewGuid()));

        RequirePermission(Permissions.Attendance.EditAttendanceMarksBulk);
        _validationService.Setup(v => v.ValidateAsync(request))
            .ThrowsAsync(new ValidationException("bad"));

        Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitBulkAsync(request, CancellationToken.None));

        _attendanceCodeService.Verify(s => s.EnsureCodesAreUsableAsync(It.IsAny<IEnumerable<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _attendanceMarkRepository.Verify(r => r.SubmitBulkAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<BulkAttendanceMarkUpsert>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void SubmitBulkAsync_PropagatesCodePolicyFailure_WithoutCallingRepo()
    {
        var request = MakeRequest(Guid.NewGuid(), new DateTime(2026, 9, 1), new DateTime(2026, 9, 5),
            Mark(Guid.NewGuid()));

        RequirePermission(Permissions.Attendance.EditAttendanceMarksBulk);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        _attendanceCodeService.Setup(s => s.EnsureCodesAreUsableAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("restricted"));

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.SubmitBulkAsync(request, CancellationToken.None));

        _attendanceMarkRepository.Verify(r => r.SubmitBulkAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<BulkAttendanceMarkUpsert>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void SubmitBulkAsync_RequiresEditAttendanceMarksBulkPermission()
    {
        // Note this is a *different* permission from EditAttendanceMarks — bulk edit is
        // gated behind the broader-scope one.
        RequirePermission(Permissions.Attendance.EditAttendanceMarksBulk, succeeds: false);

        var request = MakeRequest(Guid.NewGuid(), new DateTime(2026, 9, 1), new DateTime(2026, 9, 5),
            Mark(Guid.NewGuid()));

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.SubmitBulkAsync(request, CancellationToken.None));

        _validationService.Verify(v => v.ValidateAsync(It.IsAny<BulkAttendanceMarksRequest>()),
            Times.Never);
        _attendanceCodeService.Verify(s => s.EnsureCodesAreUsableAsync(It.IsAny<IEnumerable<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _attendanceMarkRepository.Verify(r => r.SubmitBulkAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<BulkAttendanceMarkUpsert>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
