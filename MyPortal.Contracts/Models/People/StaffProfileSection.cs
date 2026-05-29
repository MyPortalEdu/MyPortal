namespace MyPortal.Contracts.Models.People;

/// <summary>
/// The gated sections of the staff profile page. The unit of access control is the section
/// (not the field); sensitive fields live in their own sections. Serialized as the keys of the
/// capability map on the staff header response — see docs/staff-profile-access.md.
/// </summary>
public enum StaffProfileSection
{
    BasicDetails,
    EqualityAndIdentity,
    ContactMethods,
    Addresses,
    EmergencyContacts,
    Professional,
    QualificationsAndCpd,
    Employment,
    PreEmploymentChecks,
    AbsencesAndLeave,
    Timetable,
    Documents,
    Performance
}
