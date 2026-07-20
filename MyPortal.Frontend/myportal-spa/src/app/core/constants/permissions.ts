export const Permissions = {
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

export type Permission = {
  [C in keyof typeof Permissions]: (typeof Permissions)[C][keyof (typeof Permissions)[C]]
}[keyof typeof Permissions];

export const ALL_PERMISSIONS = Object.freeze(
  Object.values(Permissions).flatMap(cat => Object.values(cat))
) as readonly Permission[];
