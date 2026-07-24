namespace MyPortal.Common.Enums;

public enum DiaryEventKind : byte
{
    User = 0,
    ECActivity = 1,
    Cover = 3,
    Detention = 4,
    NCC = 5,
    PPA = 6,
    SchoolHoliday = 7,
    PublicHoliday = 8,
    // Whole-school non-pupil / INSET days.
    TeacherTraining = 9,
    ParentEvening = 10,
    // A staff member attending a scheduled training course (any staff, not only teachers).
    TrainingEvent = 11
}
