namespace MyPortal.Common.Enums;

public enum DiaryEventKind : byte
{
    User = 0,
    ECActivity = 1,
    // 2 was Lesson — dropped because lessons are backed by Sessions/SessionPeriods, not
    // DiaryEvents. The seeded row + value are removed by migration 0022. The gap is kept
    // so existing values stay stable across deployments.
    Cover = 3,
    Detention = 4,
    NCC = 5,
    PPA = 6,
    SchoolHoliday = 7,
    PublicHoliday = 8,
    TeacherTraining = 9,
    ParentEvening = 10
}
