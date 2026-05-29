using System.Data;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class StaffMemberAccessServiceTests
{
    private Mock<IAuthorizationService> _auth;
    private Mock<IStaffMemberRepository> _repo;
    private StaffMemberAccessService _service;

    private Guid _subjectId;

    [SetUp]
    public void Setup()
    {
        _auth = new Mock<IAuthorizationService>();
        _repo = new Mock<IStaffMemberRepository>();
        _service = new StaffMemberAccessService(_auth.Object,
            NullLogger<StaffMemberAccessService>.Instance, _repo.Object);

        _subjectId = Guid.NewGuid();

        // Default: no permissions. Individual tests override.
        SetPermissions();
    }

    // ----- relationship setup helpers -------------------------------------------------------

    private void AsSelf()
    {
        var personId = Guid.NewGuid();
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns(personId);
        _repo.Setup(r => r.GetByIdAsync(_subjectId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()))
            .ReturnsAsync(new StaffMember { Id = _subjectId, PersonId = personId, Code = "S1" });
    }

    private void AsLineManaged()
    {
        var viewerPersonId = Guid.NewGuid();
        var viewerStaffId = Guid.NewGuid();
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns(viewerPersonId);
        _repo.Setup(r => r.GetByIdAsync(_subjectId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()))
            .ReturnsAsync(new StaffMember { Id = _subjectId, PersonId = Guid.NewGuid(), Code = "S1" });
        _repo.Setup(r => r.GetStaffMemberIdByPersonIdAsync(viewerPersonId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(viewerStaffId);
        _repo.Setup(r => r.IsManagedByAsync(_subjectId, viewerStaffId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(true);
    }

    private void AsUnrelated()
    {
        var viewerPersonId = Guid.NewGuid();
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns(viewerPersonId);
        _repo.Setup(r => r.GetByIdAsync(_subjectId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()))
            .ReturnsAsync(new StaffMember { Id = _subjectId, PersonId = Guid.NewGuid(), Code = "S1" });
        _repo.Setup(r => r.GetStaffMemberIdByPersonIdAsync(viewerPersonId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(Guid.NewGuid());
        _repo.Setup(r => r.IsManagedByAsync(_subjectId, It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(false);
    }

    private void AsNoPersonIdentity()
    {
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns((Guid?)null);
    }

    private void SetPermissions(params string[] permissions)
    {
        _auth.Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase));
    }

    private Task<bool> CanView(StaffProfileSection section)
        => _service.CanAsync(_subjectId, section, StaffSectionVerb.View, CancellationToken.None);

    private Task<bool> CanEdit(StaffProfileSection section)
        => _service.CanAsync(_subjectId, section, StaffSectionVerb.Edit, CancellationToken.None);

    // ----- All scope: grants regardless of relationship ------------------------------------

    [Test]
    public async Task AllScope_GrantsToUnrelatedViewer()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewAllStaffEmploymentDetails);

        Assert.That(await CanView(StaffProfileSection.Employment), Is.True);
    }

    [Test]
    public async Task AllScope_GrantsToSelf()
    {
        AsSelf();
        SetPermissions(Permissions.Staff.ViewAllStaffEmploymentDetails);

        Assert.That(await CanView(StaffProfileSection.Employment), Is.True);
    }

    // ----- Own scope: self only ------------------------------------------------------------

    [Test]
    public async Task OwnScope_GrantsToSelf()
    {
        AsSelf();
        SetPermissions(Permissions.Staff.ViewOwnStaffEmploymentDetails);

        Assert.That(await CanView(StaffProfileSection.Employment), Is.True);
    }

    [Test]
    public async Task OwnScope_DeniesLineManagedViewer()
    {
        AsLineManaged();
        SetPermissions(Permissions.Staff.ViewOwnStaffEmploymentDetails);

        Assert.That(await CanView(StaffProfileSection.Employment), Is.False);
    }

    [Test]
    public async Task OwnScope_DeniesUnrelatedViewer()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewOwnStaffEmploymentDetails);

        Assert.That(await CanView(StaffProfileSection.Employment), Is.False);
    }

    // ----- Managed scope: line-managed only; never self ------------------------------------

    [Test]
    public async Task ManagedScope_GrantsToLineManagedViewer()
    {
        AsLineManaged();
        SetPermissions(Permissions.Staff.ViewManagedStaffContactMethods);

        Assert.That(await CanView(StaffProfileSection.ContactMethods), Is.True);
    }

    [Test]
    public async Task ManagedScope_DoesNotCoverSelf()
    {
        // A viewer is never their own report: holding only Managed must NOT grant on own record.
        AsSelf();
        SetPermissions(Permissions.Staff.ViewManagedStaffContactMethods);

        Assert.That(await CanView(StaffProfileSection.ContactMethods), Is.False);
    }

    [Test]
    public async Task ManagedScope_DeniesUnrelatedViewer()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewManagedStaffContactMethods);

        Assert.That(await CanView(StaffProfileSection.ContactMethods), Is.False);
    }

    // ----- Matrix gate: scope not grantable for a section ----------------------------------

    [Test]
    public async Task ScopeNotInMatrix_IsNeverGranted()
    {
        // Employment view has no Managed scope. Even a line manager holding the (non-existent)
        // managed permission string must be denied — the matrix gate stops it.
        AsLineManaged();
        SetPermissions("Staff.ViewManagedStaffEmploymentDetails");

        Assert.That(await CanView(StaffProfileSection.Employment), Is.False);
    }

    [Test]
    public async Task PerformanceHasNoOwnScope_SelfCannotView()
    {
        // Performance has no Own scope at all; a self viewer holding only the managed permission
        // can never see their own appraisal (Self is not LineManaged, and there's no Own to match).
        AsSelf();
        SetPermissions(Permissions.Staff.ViewManagedStaffPerformanceDetails);

        Assert.That(await CanView(StaffProfileSection.Performance), Is.False);
    }

    // ----- No permissions / no identity ----------------------------------------------------

    [Test]
    public async Task NoPermissions_DeniesEverything()
    {
        AsSelf();
        SetPermissions();

        Assert.That(await CanView(StaffProfileSection.BasicDetails), Is.False);
        Assert.That(await CanView(StaffProfileSection.Employment), Is.False);
        Assert.That(await CanEdit(StaffProfileSection.ContactMethods), Is.False);
    }

    [Test]
    public async Task NoPersonIdentity_AllScopeStillApplies()
    {
        AsNoPersonIdentity();
        SetPermissions(Permissions.Staff.ViewAllStaffBasicDetails);

        Assert.That(await CanView(StaffProfileSection.BasicDetails), Is.True);
    }

    [Test]
    public async Task NoPersonIdentity_OwnScopeDeniedAndNoSubjectLookup()
    {
        AsNoPersonIdentity();
        SetPermissions(Permissions.Staff.ViewOwnStaffBasicDetails);

        Assert.That(await CanView(StaffProfileSection.BasicDetails), Is.False);
        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()),
            Times.Never, "Person-less viewer should short-circuit before any subject lookup.");
    }

    // ----- Edit vs View independence -------------------------------------------------------

    [Test]
    public async Task ViewPermission_DoesNotGrantEdit()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewAllStaffBasicDetails);

        Assert.That(await CanView(StaffProfileSection.BasicDetails), Is.True);
        Assert.That(await CanEdit(StaffProfileSection.BasicDetails), Is.False);
    }

    // ----- RequireAsync --------------------------------------------------------------------

    [Test]
    public void RequireAsync_Throws_WhenDenied()
    {
        AsUnrelated();
        SetPermissions();

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.RequireAsync(_subjectId, StaffProfileSection.Employment, StaffSectionVerb.View,
                CancellationToken.None));
    }

    [Test]
    public void RequireAsync_DoesNotThrow_WhenAllowed()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewAllStaffEmploymentDetails);

        Assert.DoesNotThrowAsync(() =>
            _service.RequireAsync(_subjectId, StaffProfileSection.Employment, StaffSectionVerb.View,
                CancellationToken.None));
    }

    // ----- Capability map ------------------------------------------------------------------

    [Test]
    public async Task GetCapabilities_AllPermissionsHeld_EverySectionViewableAndEditable()
    {
        // All-scope grants regardless of relationship, and every section has an All scope for both
        // verbs, so a holder of the whole catalogue sees and can edit everything.
        AsUnrelated();
        SetPermissions(StaffMemberAccessService.AllPermissions.ToArray());

        var caps = await _service.GetCapabilitiesAsync(_subjectId, CancellationToken.None);

        Assert.That(caps, Has.Count.EqualTo(Enum.GetValues<StaffProfileSection>().Length));
        Assert.That(caps.Values.All(v => v.CanView && v.CanEdit), Is.True);
    }

    [Test]
    public async Task GetCapabilities_NoPermissions_EverythingFalse()
    {
        AsUnrelated();
        SetPermissions();

        var caps = await _service.GetCapabilitiesAsync(_subjectId, CancellationToken.None);

        Assert.That(caps.Values.All(v => v is { CanView: false, CanEdit: false }), Is.True);
    }

    [Test]
    public async Task GetCapabilities_SelfWithOwnEmployment_ViewButNotEdit()
    {
        // Employment is Own-viewable but All-only for edit — a staff member sees their own
        // employment/pay but cannot edit it.
        AsSelf();
        SetPermissions(Permissions.Staff.ViewOwnStaffEmploymentDetails);

        var caps = await _service.GetCapabilitiesAsync(_subjectId, CancellationToken.None);

        Assert.That(caps[StaffProfileSection.Employment].CanView, Is.True);
        Assert.That(caps[StaffProfileSection.Employment].CanEdit, Is.False);
    }

    // ----- Catalogue alignment: the resolver's names must match the seeded permissions ------

    [Test]
    public void AllPermissions_ExactlyMatchSeededStaffCatalogue()
    {
        var declared = typeof(Permissions.Staff)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Every permission the resolver can require must be a real, seeded constant (else it would
        // silently deny), and there should be no orphan staff constants the resolver never uses.
        Assert.That(StaffMemberAccessService.AllPermissions, Is.EquivalentTo(declared));
    }
}
