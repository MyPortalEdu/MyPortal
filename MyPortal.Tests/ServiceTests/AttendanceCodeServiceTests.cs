using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Attendance;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class AttendanceCodeServiceTests
{
    private Mock<IAuthorizationService> _authorizationService;
    private Mock<ILogger<AttendanceCodeService>> _logger;
    private Mock<IAttendanceCodeRepository> _attendanceCodeRepository;

    private AttendanceCodeService _service;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<AttendanceCodeService>>(MockBehavior.Loose);
        _attendanceCodeRepository = new Mock<IAttendanceCodeRepository>(MockBehavior.Strict);

        _service = new AttendanceCodeService(
            _authorizationService.Object,
            _logger.Object,
            _attendanceCodeRepository.Object);
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

    private void StubHasPermission(string permission, bool granted)
    {
        _authorizationService.Setup(a =>
                a.HasPermissionAsync(permission, It.IsAny<CancellationToken>()))
            .ReturnsAsync(granted);
    }

    private static AttendanceCode Code(Guid id, string codeChar = "/", bool active = true,
        bool restricted = false)
        => new()
        {
            Id = id,
            Code = codeChar,
            Description = "code",
            AttendanceCodeTypeId = Guid.NewGuid(),
            IsActive = active,
            IsRestricted = restricted,
            IsSystem = false
        };

    // ─── GetActiveAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task GetActiveAsync_MapsEntitiesToResponses_AndIncludesRestricted()
    {
        // Restricted codes must be returned to every caller — the UI is responsible
        // for greying them out for users without UseRestrictedCodes.
        var present = Code(Guid.NewGuid(), "/", restricted: false);
        var locked  = Code(Guid.NewGuid(), "B", restricted: true);

        RequirePermission(Permissions.Attendance.ViewAttendanceMarks);
        _attendanceCodeRepository.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttendanceCode> { present, locked });

        var result = await _service.GetActiveAsync(CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Single(c => c.Code == "/").IsRestricted, Is.False);
        Assert.That(result.Single(c => c.Code == "B").IsRestricted, Is.True);
    }

    [Test]
    public void GetActiveAsync_RequiresViewPermission()
    {
        RequirePermission(Permissions.Attendance.ViewAttendanceMarks, succeeds: false);

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.GetActiveAsync(CancellationToken.None));

        // Permission must short-circuit before the repo is queried.
        _attendanceCodeRepository.Verify(r => r.GetActiveAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─── EnsureCodesAreUsableAsync ───────────────────────────────────────────

    [Test]
    public async Task EnsureCodesAreUsableAsync_NoOpsOnEmptyInput()
    {
        // Empty input means "nothing to validate" — the repo should not be hit.
        await _service.EnsureCodesAreUsableAsync(Array.Empty<Guid>(), CancellationToken.None);

        _attendanceCodeRepository.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EnsureCodesAreUsableAsync_DedupesCodeIds()
    {
        // Same code referenced by many marks shouldn't blow up the lookup or surface twice.
        var id = Guid.NewGuid();
        IEnumerable<Guid>? capturedIds = null;

        _attendanceCodeRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Guid>, CancellationToken>((ids, _) => capturedIds = ids.ToList())
            .ReturnsAsync(new List<AttendanceCode> { Code(id) });

        await _service.EnsureCodesAreUsableAsync(new[] { id, id, id }, CancellationToken.None);

        Assert.That(capturedIds, Is.Not.Null);
        Assert.That(capturedIds!.Count(), Is.EqualTo(1));
    }

    [Test]
    public void EnsureCodesAreUsableAsync_ThrowsNotFound_WhenAnyCodeUnknown()
    {
        var unknown = Guid.NewGuid();

        _attendanceCodeRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttendanceCode>());  // repo returned no rows for the requested ids

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            _service.EnsureCodesAreUsableAsync(new[] { unknown }, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain(unknown.ToString()));
    }

    [Test]
    public void EnsureCodesAreUsableAsync_ThrowsInvalidOperation_WhenAnyCodeInactive()
    {
        var inactive = Code(Guid.NewGuid(), "X", active: false);

        _attendanceCodeRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttendanceCode> { inactive });

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.EnsureCodesAreUsableAsync(new[] { inactive.Id }, CancellationToken.None));
    }

    [Test]
    public void EnsureCodesAreUsableAsync_ThrowsForbidden_WhenRestrictedAndCallerLacksPermission()
    {
        var restricted = Code(Guid.NewGuid(), "B", restricted: true);

        _attendanceCodeRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttendanceCode> { restricted });
        StubHasPermission(Permissions.Attendance.UseRestrictedCodes, granted: false);

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.EnsureCodesAreUsableAsync(new[] { restricted.Id }, CancellationToken.None));
    }

    [Test]
    public async Task EnsureCodesAreUsableAsync_AllowsRestricted_WhenCallerHasPermission()
    {
        var restricted = Code(Guid.NewGuid(), "B", restricted: true);

        _attendanceCodeRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttendanceCode> { restricted });
        StubHasPermission(Permissions.Attendance.UseRestrictedCodes, granted: true);

        await _service.EnsureCodesAreUsableAsync(new[] { restricted.Id }, CancellationToken.None);
    }

    [Test]
    public async Task EnsureCodesAreUsableAsync_DoesNotCheckUseRestrictedCodes_WhenAllCodesUnrestricted()
    {
        // Avoid hitting the auth service for the common "all-normal-codes" path.
        var ordinary = Code(Guid.NewGuid(), "/");

        _attendanceCodeRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttendanceCode> { ordinary });

        await _service.EnsureCodesAreUsableAsync(new[] { ordinary.Id }, CancellationToken.None);

        _authorizationService.Verify(a =>
            a.HasPermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
