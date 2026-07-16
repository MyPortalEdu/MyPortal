export const Permissions = {
  // Object key matches the C# `Permissions.SystemAdmin` class name (which avoids the `System`
  // namespace collision); the wire-string values keep the "System." prefix to match the
  // server's permission rows.
  SystemAdmin: {
    ViewUsers:           'System.ViewUsers',
    EditUsers:           'System.EditUsers',
    ViewRoles:           'System.ViewRoles',
    EditRoles:           'System.EditRoles',
    ViewGroups:          'System.ViewGroups',
    EditGroups:          'System.EditGroups',
    BulletinSettings:    'System.BulletinSettings'
  },
  Agencies: {
    ViewAgencies: 'Agencies.ViewAgencies',
    EditAgencies: 'Agencies.EditAgencies',
  },
  School: {
    ViewSchoolBulletins:   'School.ViewSchoolBulletins',
    EditSchoolBulletins:   'School.EditSchoolBulletins',
    PinSchoolBulletins:    'School.PinSchoolBulletins',
  },
  Curriculum: {
    ViewAcademicYears: 'Curriculum.ViewAcademicYears',
    EditAcademicYears: 'Curriculum.EditAcademicYears',
  },
  // Scoped staff-profile permissions: Staff.{Verb}{Scope}Staff{Area}.
  // See docs/staff-profile-access.md.
  Staff: {
    ViewOwnStaffBasicDetails:            'Staff.ViewOwnStaffBasicDetails',
    ViewManagedStaffBasicDetails:        'Staff.ViewManagedStaffBasicDetails',
    ViewAllStaffBasicDetails:            'Staff.ViewAllStaffBasicDetails',
    EditManagedStaffBasicDetails:        'Staff.EditManagedStaffBasicDetails',
    EditAllStaffBasicDetails:            'Staff.EditAllStaffBasicDetails',

    ViewOwnStaffEqualityDetails:         'Staff.ViewOwnStaffEqualityDetails',
    ViewAllStaffEqualityDetails:         'Staff.ViewAllStaffEqualityDetails',
    EditAllStaffEqualityDetails:         'Staff.EditAllStaffEqualityDetails',

    ViewOwnStaffProfessionalDetails:     'Staff.ViewOwnStaffProfessionalDetails',
    ViewManagedStaffProfessionalDetails: 'Staff.ViewManagedStaffProfessionalDetails',
    ViewAllStaffProfessionalDetails:     'Staff.ViewAllStaffProfessionalDetails',
    EditManagedStaffProfessionalDetails: 'Staff.EditManagedStaffProfessionalDetails',
    EditAllStaffProfessionalDetails:     'Staff.EditAllStaffProfessionalDetails',

    ViewOwnStaffEmploymentDetails:       'Staff.ViewOwnStaffEmploymentDetails',
    ViewAllStaffEmploymentDetails:       'Staff.ViewAllStaffEmploymentDetails',
    EditAllStaffEmploymentDetails:       'Staff.EditAllStaffEmploymentDetails',

    ViewAllStaffPreEmploymentChecks:     'Staff.ViewAllStaffPreEmploymentChecks',
    EditAllStaffPreEmploymentChecks:     'Staff.EditAllStaffPreEmploymentChecks',

    ViewOwnStaffAbsences:                'Staff.ViewOwnStaffAbsences',
    ViewManagedStaffAbsences:            'Staff.ViewManagedStaffAbsences',
    ViewAllStaffAbsences:                'Staff.ViewAllStaffAbsences',
    EditManagedStaffAbsences:            'Staff.EditManagedStaffAbsences',
    EditAllStaffAbsences:                'Staff.EditAllStaffAbsences',

    ViewOwnStaffTimetable:               'Staff.ViewOwnStaffTimetable',
    ViewManagedStaffTimetable:           'Staff.ViewManagedStaffTimetable',
    ViewAllStaffTimetable:               'Staff.ViewAllStaffTimetable',
    EditAllStaffTimetable:               'Staff.EditAllStaffTimetable',

    ViewOwnStaffDocuments:               'Staff.ViewOwnStaffDocuments',
    ViewManagedStaffDocuments:           'Staff.ViewManagedStaffDocuments',
    ViewAllStaffDocuments:               'Staff.ViewAllStaffDocuments',
    EditOwnStaffDocuments:               'Staff.EditOwnStaffDocuments',
    EditManagedStaffDocuments:           'Staff.EditManagedStaffDocuments',
    EditAllStaffDocuments:               'Staff.EditAllStaffDocuments',

    ViewManagedStaffPerformanceDetails:  'Staff.ViewManagedStaffPerformanceDetails',
    ViewAllStaffPerformanceDetails:      'Staff.ViewAllStaffPerformanceDetails',
    EditManagedStaffPerformanceDetails:  'Staff.EditManagedStaffPerformanceDetails',
    EditAllStaffPerformanceDetails:      'Staff.EditAllStaffPerformanceDetails',
  },
} as const;

// Flatten to a union of all permission string literals across every category.
// Distributes over keys so adding a second category yields a union, not `never`.
export type Permission = {
  [C in keyof typeof Permissions]: (typeof Permissions)[C][keyof (typeof Permissions)[C]]
}[keyof typeof Permissions];

export const ALL_PERMISSIONS = Object.freeze(
  Object.values(Permissions).flatMap(cat => Object.values(cat))
) as readonly Permission[];
