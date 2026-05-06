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
public class RegisterServiceTests
{
    private Mock<IAuthorizationService> _authorizationService;
    private Mock<ILogger<RegisterService>> _logger;
    private Mock<IRegisterRepository> _registerRepository;
    private Mock<IAttendanceCodeService> _attendanceCodeService;
    private Mock<IValidationService> _validationService;

    private RegisterService _service;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<RegisterService>>(MockBehavior.Loose);
        _registerRepository = new Mock<IRegisterRepository>(MockBehavior.Strict);
        _attendanceCodeService = new Mock<IAttendanceCodeService>(MockBehavior.Strict);
        _validationService = new Mock<IValidationService>(MockBehavior.Strict);

        _service = new RegisterService(
            _authorizationService.Object,
            _logger.Object,
            _registerRepository.Object,
            _attendanceCodeService.Object,
            _validationService.Object
        );
    }

    /// <summary>
    /// Default stub: code policy passes silently. Tests that need a failing policy
    /// override this with their own setup.
    /// </summary>
    private void StubAllCodesValid()
    {
        _attendanceCodeService
            .Setup(s => s.EnsureCodesAreUsableAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void StubCodePolicyThrows(Exception ex)
    {
        _attendanceCodeService
            .Setup(s => s.EnsureCodesAreUsableAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(ex);
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

    private static SubmitRegisterRequest MakeRequest(params Guid[] studentIds)
    {
        var marks = studentIds
            .Select(id => new SubmitMarkRequest
                { StudentId = id, AttendanceCodeId = Guid.NewGuid() })
            .ToList<SubmitMarkRequest>();
        return new SubmitRegisterRequest { Marks = marks };
    }

    // ─── GetLessonRegisterAsync ──────────────────────────────────────────────

    [Test]
    public async Task GetLessonRegisterAsync_ReturnsRegister_WhenRepoReturnsResult()
    {
        var sessionPeriodId = Guid.NewGuid();
        var weekId = Guid.NewGuid();
        var dto = new RegisterResponse { GroupCode = "7L/Ma1" };

        RequirePermission(Permissions.Attendance.ViewAttendanceMarks);
        _registerRepository.Setup(r => r.GetLessonRegisterAsync(sessionPeriodId, weekId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _service.GetLessonRegisterAsync(sessionPeriodId, weekId, CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void GetLessonRegisterAsync_ThrowsNotFound_WhenRepoReturnsNull()
    {
        var sessionPeriodId = Guid.NewGuid();
        var weekId = Guid.NewGuid();

        RequirePermission(Permissions.Attendance.ViewAttendanceMarks);
        _registerRepository.Setup(r => r.GetLessonRegisterAsync(sessionPeriodId, weekId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegisterResponse?)null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetLessonRegisterAsync(sessionPeriodId, weekId, CancellationToken.None));
    }

    [Test]
    public void GetLessonRegisterAsync_RequiresViewPermission()
    {
        RequirePermission(Permissions.Attendance.ViewAttendanceMarks, succeeds: false);

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.GetLessonRegisterAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        // Permission must short-circuit before any repo lookup.
        _registerRepository.Verify(r => r.GetLessonRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── GetRegGroupRegisterAsync ────────────────────────────────────────────

    [Test]
    public async Task GetRegGroupRegisterAsync_ReturnsRegister_WhenRepoReturnsResult()
    {
        var regGroupId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        var weekId = Guid.NewGuid();
        var dto = new RegisterResponse { GroupCode = "7A" };

        RequirePermission(Permissions.Attendance.ViewAttendanceMarks);
        _registerRepository.Setup(r => r.GetRegGroupRegisterAsync(regGroupId, periodId, weekId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _service.GetRegGroupRegisterAsync(regGroupId, periodId, weekId, CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void GetRegGroupRegisterAsync_ThrowsNotFound_WhenRepoReturnsNull()
    {
        var regGroupId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        var weekId = Guid.NewGuid();

        RequirePermission(Permissions.Attendance.ViewAttendanceMarks);
        _registerRepository.Setup(r => r.GetRegGroupRegisterAsync(regGroupId, periodId, weekId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegisterResponse?)null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetRegGroupRegisterAsync(regGroupId, periodId, weekId, CancellationToken.None));
    }

    [Test]
    public void GetRegGroupRegisterAsync_RequiresViewPermission()
    {
        RequirePermission(Permissions.Attendance.ViewAttendanceMarks, succeeds: false);

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.GetRegGroupRegisterAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                CancellationToken.None));

        _registerRepository.Verify(r => r.GetRegGroupRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── SubmitLessonRegisterAsync ───────────────────────────────────────────

    [Test]
    public async Task SubmitLessonRegisterAsync_ForwardsMarksToRepo_AfterValidation()
    {
        var sessionPeriodId = Guid.NewGuid();
        var weekId = Guid.NewGuid();
        var request = MakeRequest(Guid.NewGuid(), Guid.NewGuid());

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        StubAllCodesValid();
        _registerRepository.Setup(r => r.SubmitLessonRegisterAsync(sessionPeriodId, weekId,
                It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SubmitLessonRegisterAsync(sessionPeriodId, weekId, request, CancellationToken.None);

        _registerRepository.Verify(r => r.SubmitLessonRegisterAsync(sessionPeriodId, weekId,
            It.Is<IReadOnlyCollection<SubmitMarkRequest>>(m => m.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SubmitLessonRegisterAsync_AllowsEmptyMarks()
    {
        // The teacher should be able to "open and immediately close" a register without marks.
        var sessionPeriodId = Guid.NewGuid();
        var weekId = Guid.NewGuid();
        var request = new SubmitRegisterRequest { Marks = new List<SubmitMarkRequest>() };

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        StubAllCodesValid();
        _registerRepository.Setup(r => r.SubmitLessonRegisterAsync(sessionPeriodId, weekId,
                It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SubmitLessonRegisterAsync(sessionPeriodId, weekId, request, CancellationToken.None);

        _registerRepository.Verify(r => r.SubmitLessonRegisterAsync(sessionPeriodId, weekId,
            It.Is<IReadOnlyCollection<SubmitMarkRequest>>(m => m.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void SubmitLessonRegisterAsync_PropagatesValidationFailure_WithoutCallingRepo()
    {
        var request = MakeRequest(Guid.NewGuid());

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request))
            .ThrowsAsync(new ValidationException("bad"));

        Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitLessonRegisterAsync(Guid.NewGuid(), Guid.NewGuid(), request, CancellationToken.None));

        _registerRepository.Verify(r => r.SubmitLessonRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void SubmitLessonRegisterAsync_RejectsInactiveCode()
    {
        var request = new SubmitRegisterRequest
        {
            Marks = new List<SubmitMarkRequest>
                { new() { StudentId = Guid.NewGuid(), AttendanceCodeId = Guid.NewGuid() } }
        };

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        StubCodePolicyThrows(new InvalidOperationException("Cannot submit inactive attendance code(s): X"));

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SubmitLessonRegisterAsync(Guid.NewGuid(), Guid.NewGuid(), request, CancellationToken.None));

        // Repo write must not happen when the policy fails — the row would survive in DB.
        _registerRepository.Verify(r => r.SubmitLessonRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void SubmitLessonRegisterAsync_RejectsUnknownCode()
    {
        var request = MakeRequest(Guid.NewGuid());

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        StubCodePolicyThrows(new NotFoundException("Unknown attendance code(s)"));

        Assert.ThrowsAsync<NotFoundException>(() =>
            _service.SubmitLessonRegisterAsync(Guid.NewGuid(), Guid.NewGuid(), request, CancellationToken.None));

        _registerRepository.Verify(r => r.SubmitLessonRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SubmitLessonRegisterAsync_AllowsRestrictedCode_WhenPolicyAccepts()
    {
        // Restricted-code auth lives inside IAttendanceCodeService now; this test asserts
        // RegisterService delegates to it and proceeds with the write when it passes.
        var sessionPeriodId = Guid.NewGuid();
        var weekId = Guid.NewGuid();
        var request = MakeRequest(Guid.NewGuid());

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        StubAllCodesValid();
        _registerRepository.Setup(r => r.SubmitLessonRegisterAsync(sessionPeriodId, weekId,
                It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SubmitLessonRegisterAsync(sessionPeriodId, weekId, request, CancellationToken.None);

        _registerRepository.Verify(r => r.SubmitLessonRegisterAsync(sessionPeriodId, weekId,
            It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void SubmitLessonRegisterAsync_RejectsRestrictedCode_WhenPolicyForbids()
    {
        var request = MakeRequest(Guid.NewGuid());

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        StubCodePolicyThrows(new ForbiddenException("restricted code"));

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.SubmitLessonRegisterAsync(Guid.NewGuid(), Guid.NewGuid(), request, CancellationToken.None));

        _registerRepository.Verify(r => r.SubmitLessonRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void SubmitLessonRegisterAsync_RequiresEditPermission()
    {
        RequirePermission(Permissions.Attendance.EditAttendanceMarks, succeeds: false);

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.SubmitLessonRegisterAsync(Guid.NewGuid(), Guid.NewGuid(),
                MakeRequest(Guid.NewGuid()), CancellationToken.None));

        // Permission must short-circuit before validation or repo writes.
        _validationService.Verify(v => v.ValidateAsync(It.IsAny<SubmitRegisterRequest>()), Times.Never);
        _registerRepository.Verify(r => r.SubmitLessonRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── SubmitRegGroupRegisterAsync ─────────────────────────────────────────

    [Test]
    public async Task SubmitRegGroupRegisterAsync_ForwardsMarksToRepo_AfterValidation()
    {
        var regGroupId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        var weekId = Guid.NewGuid();
        var request = MakeRequest(Guid.NewGuid());

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request)).Returns(Task.CompletedTask);
        StubAllCodesValid();
        _registerRepository.Setup(r => r.SubmitRegGroupRegisterAsync(regGroupId, periodId, weekId,
                It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SubmitRegGroupRegisterAsync(regGroupId, periodId, weekId, request,
            CancellationToken.None);

        _registerRepository.Verify(r => r.SubmitRegGroupRegisterAsync(regGroupId, periodId, weekId,
            It.Is<IReadOnlyCollection<SubmitMarkRequest>>(m => m.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void SubmitRegGroupRegisterAsync_PropagatesValidationFailure_WithoutCallingRepo()
    {
        var request = MakeRequest(Guid.NewGuid());

        RequirePermission(Permissions.Attendance.EditAttendanceMarks);
        _validationService.Setup(v => v.ValidateAsync(request))
            .ThrowsAsync(new ValidationException("bad"));

        Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitRegGroupRegisterAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), request,
                CancellationToken.None));

        _registerRepository.Verify(r => r.SubmitRegGroupRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void SubmitRegGroupRegisterAsync_RequiresEditPermission()
    {
        RequirePermission(Permissions.Attendance.EditAttendanceMarks, succeeds: false);

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.SubmitRegGroupRegisterAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                MakeRequest(Guid.NewGuid()), CancellationToken.None));

        _validationService.Verify(v => v.ValidateAsync(It.IsAny<SubmitRegisterRequest>()), Times.Never);
        _registerRepository.Verify(r => r.SubmitRegGroupRegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<SubmitMarkRequest>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
