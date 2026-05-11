namespace MyPortal.Common.Constants;

/// <summary>
/// Well-known DiaryEventType IDs seeded by 0004_seed_uk_data.sql. Each row is keyed by
/// <see cref="Enums.DiaryEventKind"/> and marked IsSystem=1 (except General), so these IDs
/// are stable across deployments and not deletable through the UI.
/// </summary>
public static class DiaryEventTypes
{
    public static readonly Guid General         = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F2A");
    public static readonly Guid EcActivity      = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F22");
    // …F23 was Lesson — removed; lessons are backed by Sessions, not DiaryEvents (mig 0022).
    public static readonly Guid Cover           = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F24");
    public static readonly Guid Detention       = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F25");
    public static readonly Guid Ncc             = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F26");
    public static readonly Guid Ppa             = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F27");
    public static readonly Guid SchoolHoliday   = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F28");
    public static readonly Guid PublicHoliday   = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F2C");
    public static readonly Guid TeacherTraining = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F29");
    public static readonly Guid ParentEvening   = Guid.Parse("84E9DDA4-1BCB-4A2F-8082-FCE51DD04F2B");
}
