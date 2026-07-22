namespace MyPortal.Auth.Constants;

/// <summary>
/// Wire strings for permissions, grouped by area. A permission's portal audience (Staff / Student /
/// Parent) is a DB attribute set by its seed migration — it is not encoded in the constant — so this
/// stays a flat registry. Student and parent portal permissions get their own groups when those
/// features are built.
/// </summary>
public static class Permissions
{
    // Permissions to be uncommented as and when features are implemented
    public static class Admissions
    {
        // public const string ViewApplications   = "Admissions.ViewApplications";
        // public const string EditApplications   = "Admissions.EditApplications";
        // public const string ViewEnquiries      = "Admissions.ViewEnquiries";
        // public const string EditEnquiries      = "Admissions.EditEnquiries";
        // public const string ViewInterviews     = "Admissions.ViewInterviews";
        // public const string EditInterviews     = "Admissions.EditInterviews";
    }

    public static class Agencies
    {
        public const string ViewAgencies = "Agencies.ViewAgencies";
        public const string EditAgencies = "Agencies.EditAgencies";
    }

    public static class Assessment
    {
        // public const string ViewAspects           = "Assessment.ViewAspects";
        // public const string EditAspects           = "Assessment.EditAspects";
        // public const string ViewExamBaseData      = "Assessment.ViewExamBaseData";
        // public const string EditExamBaseData      = "Assessment.EditExamBaseData";
        // public const string RunExamAsst           = "Assessment.RunExamAsst";
        // public const string ViewGradeSets         = "Assessment.ViewGradeSets";
        // public const string EditGradeSets         = "Assessment.EditGradeSets";
        // public const string ViewMarksheetTemplates= "Assessment.ViewMarksheetTemplates";
        // public const string EditMarksheetTemplates= "Assessment.EditMarksheetTemplates";
        // public const string ViewResultSets        = "Assessment.ViewResultSets";
        // public const string EditResultSets        = "Assessment.EditResultSets";
        // public const string ViewOwnMarksheets     = "Assessment.ViewOwnMarksheets";
        // public const string ViewAllMarksheets     = "Assessment.ViewAllMarksheets";
        // public const string UpdateOwnMarksheets   = "Assessment.UpdateOwnMarksheets";
        // public const string UpdateAllMarksheets   = "Assessment.UpdateAllMarksheets";
        // public const string ViewResults           = "Assessment.ViewResults";
        // public const string ViewEmbargoedResults  = "Assessment.ViewEmbargoedResults";
        // public const string EditResults           = "Assessment.EditResults";
    }

    public static class Attendance
    {
        public const string ViewAttendanceSetup     = "Attendance.ViewAttendanceSetup";
        public const string EditAttendanceSetup     = "Attendance.EditAttendanceSetup";
        public const string ViewAttendanceMarks     = "Attendance.ViewAttendanceMarks";
        public const string EditAttendanceMarks     = "Attendance.EditAttendanceMarks";
        public const string EditAttendanceMarksBulk = "Attendance.EditAttendanceMarksBulk";
        public const string UseRestrictedCodes      = "Attendance.UseRestrictedCodes";
    }

    public static class Timetable
    {
        public const string ViewTimetables = "Timetable.ViewTimetables";
        public const string EditTimetables = "Timetable.EditTimetables";
    }

    public static class Behaviour
    {
        // public const string ViewAchievements      = "Behaviour.ViewAchievements";
        // public const string EditAchievements      = "Behaviour.EditAchievements";
        // public const string ViewIncidents         = "Behaviour.ViewIncidents";
        // public const string EditIncidents         = "Behaviour.EditIncidents";
        // public const string ViewDetentions        = "Behaviour.ViewDetentions";
        // public const string EditDetentions        = "Behaviour.EditDetentions";
        // public const string ViewExclusions        = "Behaviour.ViewExclusions";
        // public const string EditExclusions        = "Behaviour.EditExclusions";
        // public const string ViewReportCards       = "Behaviour.ViewReportCards";
        // public const string EditReportCards       = "Behaviour.EditReportCards";
        // public const string AddRemoveReportCards  = "Behaviour.AddRemoveReportCards";
    }

    public static class Curriculum
    {
        // public const string AcademicStructure = "Curriculum.AcademicStructure";
        public const string ViewAcademicYears = "Curriculum.ViewAcademicYears";
        public const string EditAcademicYears = "Curriculum.EditAcademicYears";
        // public const string ArrangeCover      = "Curriculum.ArrangeCover";
        // public const string ViewHomework      = "Curriculum.ViewHomework";
        // public const string EditHomework      = "Curriculum.EditHomework";
        // public const string ViewLessonPlans   = "Curriculum.ViewLessonPlans";
        // public const string EditLessonPlans   = "Curriculum.EditLessonPlans";
        // public const string ViewStudyTopics   = "Curriculum.ViewStudyTopics";
        // public const string EditStudyTopics   = "Curriculum.EditStudyTopics";
    }

    public static class Finance
    {
        // public const string ViewAccounts  = "Finance.ViewAccounts";
        // public const string EditAccounts  = "Finance.EditAccounts";
        // public const string ViewProducts  = "Finance.ViewProducts";
        // public const string EditProducts  = "Finance.EditProducts";
        // public const string ViewBills     = "Finance.ViewBills";
        // public const string EditBills     = "Finance.EditBills";
        // public const string ViewCharges   = "Finance.ViewCharges";
        // public const string EditCharges   = "Finance.EditCharges";
        // public const string ViewDiscounts = "Finance.ViewDiscounts";
        // public const string EditDiscounts = "Finance.EditDiscounts";
    }

    public static class People
    {
        // public const string ViewAgentDetails                = "People.ViewAgentDetails";
        // public const string EditAgentDetails                = "People.EditAgentDetails";
        // public const string ViewContactDetails              = "People.ViewContactDetails";
        // public const string EditContactDetails              = "People.EditContactDetails";
        // public const string ViewContactTasks                = "People.ViewContactTasks";
        // public const string EditContactTasks                = "People.EditContactTasks";
    }

    public static class Profiles
    {
        // public const string ViewCommentBanks      = "Profiles.ViewCommentBanks";
        // public const string EditCommentBanks      = "Profiles.EditCommentBanks";
        // public const string ViewReports           = "Profiles.ViewReports";
        // public const string EditOwnReports        = "Profiles.EditOwnReports";
        // public const string EditAllReports        = "Profiles.EditAllReports";
        // public const string ViewReportingSessions = "Profiles.ViewReportingSessions";
        // public const string EditReportingSessions = "Profiles.EditReportingSessions";
    }

    public static class School
    {
        public const string ViewPastoralStructure       = "School.ViewPastoralStructure";
        public const string EditPastoralStructure       = "School.EditPastoralStructure";
        // public const string ViewRooms               = "School.ViewRooms";
        // public const string EditRooms               = "School.EditRooms";
        // public const string ViewSchoolDetails       = "School.ViewSchoolDetails";
        // public const string EditSchoolDetails       = "School.EditSchoolDetails";
        // public const string ViewSchoolDiary         = "School.ViewSchoolDiary";
        // public const string EditSchoolDiary         = "School.EditSchoolDiary";
        // public const string ViewSchoolDocuments     = "School.ViewSchoolDocuments";
        // public const string EditSchoolDocuments     = "School.EditSchoolDocuments";
        public const string ViewSchoolBulletins     = "School.ViewSchoolBulletins";
        public const string EditSchoolBulletins     = "School.EditSchoolBulletins";
        public const string PinSchoolBulletins      = "School.PinSchoolBulletins";
    }

    public static class Student
    {
        // public const string ViewSenDetails      = "Student.ViewSenDetails";
        // public const string EditSenDetails      = "Student.EditSenDetails";
        // public const string ViewStudentDetails  = "Student.ViewStudentDetails";
        // public const string EditStudentDetails  = "Student.EditStudentDetails";
        // public const string ViewStudentLogNotes = "Student.ViewStudentLogNotes";
        // public const string EditStudentLogNotes = "Student.EditStudentLogNotes";
        // public const string ViewStudentTasks    = "Student.ViewStudentTasks";
        // public const string EditStudentTasks    = "Student.EditStudentTasks";
        // public const string ViewStudentDocuments= "Student.ViewStudentDocuments";
        // public const string EditStudentDocuments= "Student.EditStudentDocuments";
        // public const string ViewMedicalEvents   = "Student.ViewMedicalEvents";
        // public const string EditMedicalEvents   = "Student.EditMedicalEvents";
        // public const string ViewFinanceDetails  = "Student.ViewFinanceDetails";
        // public const string EditFinanceDetails  = "Student.EditFinanceDetails";
    }

    // Scoped staff-profile permissions: Staff.{Verb}{Scope}Staff{Area}, Scope ∈ {Own, Managed, All}.
    // Areas are permission domains, not UI sections. See docs/staff-profile-access.md.
    public static class Staff
    {
        // Basic details (incl. contact methods, addresses, emergency contacts). No EditOwn:
        // legal-name/DOB changes route through HR.
        public const string ViewOwnStaffBasicDetails            = "Staff.ViewOwnStaffBasicDetails";
        public const string ViewManagedStaffBasicDetails        = "Staff.ViewManagedStaffBasicDetails";
        public const string ViewAllStaffBasicDetails            = "Staff.ViewAllStaffBasicDetails";
        public const string EditManagedStaffBasicDetails        = "Staff.EditManagedStaffBasicDetails";
        public const string EditAllStaffBasicDetails            = "Staff.EditAllStaffBasicDetails";

        // Equality & identity (ethnicity, religion, disability, NI) — GDPR special-category;
        // no Managed scope.
        public const string ViewOwnStaffEqualityDetails         = "Staff.ViewOwnStaffEqualityDetails";
        public const string ViewAllStaffEqualityDetails         = "Staff.ViewAllStaffEqualityDetails";
        public const string EditAllStaffEqualityDetails         = "Staff.EditAllStaffEqualityDetails";

        // Professional (QTS, TRN, subjects, qualifications, CPD). No EditOwn — HR-verified.
        public const string ViewOwnStaffProfessionalDetails     = "Staff.ViewOwnStaffProfessionalDetails";
        public const string ViewManagedStaffProfessionalDetails = "Staff.ViewManagedStaffProfessionalDetails";
        public const string ViewAllStaffProfessionalDetails     = "Staff.ViewAllStaffProfessionalDetails";
        public const string EditManagedStaffProfessionalDetails = "Staff.EditManagedStaffProfessionalDetails";
        public const string EditAllStaffProfessionalDetails     = "Staff.EditAllStaffProfessionalDetails";

        // Employment & contract (incl. salary, bank). Crown jewels: All-only edit, no Managed view.
        public const string ViewOwnStaffEmploymentDetails       = "Staff.ViewOwnStaffEmploymentDetails";
        public const string ViewAllStaffEmploymentDetails       = "Staff.ViewAllStaffEmploymentDetails";
        public const string EditAllStaffEmploymentDetails       = "Staff.EditAllStaffEmploymentDetails";

        // Pre-employment checks (DBS, right to work). Safeguarding/HR; All-only.
        public const string ViewAllStaffPreEmploymentChecks     = "Staff.ViewAllStaffPreEmploymentChecks";
        public const string EditAllStaffPreEmploymentChecks     = "Staff.EditAllStaffPreEmploymentChecks";

        // Emergency contacts / next of kin. HR-maintained; All-only.
        public const string ViewAllStaffEmergencyContacts       = "Staff.ViewAllStaffEmergencyContacts";
        public const string EditAllStaffEmergencyContacts       = "Staff.EditAllStaffEmergencyContacts";

        // Designated responsibilities (DSL, First Aider, SENCO, …). View self/manager/HR; HR edit only.
        public const string ViewOwnStaffResponsibilities        = "Staff.ViewOwnStaffResponsibilities";
        public const string ViewManagedStaffResponsibilities    = "Staff.ViewManagedStaffResponsibilities";
        public const string ViewAllStaffResponsibilities        = "Staff.ViewAllStaffResponsibilities";
        public const string EditAllStaffResponsibilities        = "Staff.EditAllStaffResponsibilities";

        // Absences & leave (health data). No EditOwn — you don't self-mark sick.
        public const string ViewOwnStaffAbsences                = "Staff.ViewOwnStaffAbsences";
        public const string ViewManagedStaffAbsences            = "Staff.ViewManagedStaffAbsences";
        public const string ViewAllStaffAbsences                = "Staff.ViewAllStaffAbsences";
        public const string EditManagedStaffAbsences            = "Staff.EditManagedStaffAbsences";
        public const string EditAllStaffAbsences                = "Staff.EditAllStaffAbsences";

        // Timetable is view-only on a staff profile — editing is a whole-school scheduling action
        // (Timetable.EditTimetables), never per staff member.
        public const string ViewOwnStaffTimetable               = "Staff.ViewOwnStaffTimetable";
        public const string ViewManagedStaffTimetable           = "Staff.ViewManagedStaffTimetable";
        public const string ViewAllStaffTimetable               = "Staff.ViewAllStaffTimetable";

        // Documents.
        public const string ViewOwnStaffDocuments               = "Staff.ViewOwnStaffDocuments";
        public const string ViewManagedStaffDocuments           = "Staff.ViewManagedStaffDocuments";
        public const string ViewAllStaffDocuments               = "Staff.ViewAllStaffDocuments";
        public const string EditOwnStaffDocuments               = "Staff.EditOwnStaffDocuments";
        public const string EditManagedStaffDocuments           = "Staff.EditManagedStaffDocuments";
        public const string EditAllStaffDocuments               = "Staff.EditAllStaffDocuments";

        // Performance / appraisal. No Own scope.
        public const string ViewManagedStaffPerformanceDetails  = "Staff.ViewManagedStaffPerformanceDetails";
        public const string ViewAllStaffPerformanceDetails      = "Staff.ViewAllStaffPerformanceDetails";
        public const string EditManagedStaffPerformanceDetails  = "Staff.EditManagedStaffPerformanceDetails";
        public const string EditAllStaffPerformanceDetails      = "Staff.EditAllStaffPerformanceDetails";

        // Staff Setup — school-level HR reference data (established posts & vacancies, and in time
        // service terms, pay scales and superannuation). Not per-staff-member, so no scopes.
        public const string ViewStaffSetup                      = "Staff.ViewStaffSetup";
        public const string EditStaffSetup                      = "Staff.EditStaffSetup";

        // Not yet modelled — kept as a marker for future slices.
        // public const string ViewAllStaffTasks               = "Staff.ViewAllStaffTasks";
        // public const string ViewManagedStaffTasks           = "Staff.ViewManagedStaffTasks";
        // public const string EditAllStaffTasks               = "Staff.EditAllStaffTasks";
        // public const string EditManagedStaffTasks           = "Staff.EditManagedStaffTasks";
        // public const string ViewTrainingCourses             = "Staff.ViewTrainingCourses";
        // public const string EditTrainingCourses             = "Staff.EditTrainingCourses";
    }

    // Class name avoids the `System` namespace; the wire-string values keep the "System."
    // prefix so existing rows in dbo.Permissions and any pre-issued tokens still match.
    public static class SystemAdmin
    {
        public const string ViewUsers  = "System.ViewUsers";
        public const string EditUsers  = "System.EditUsers";
        public const string ViewRoles = "System.ViewRoles";
        public const string EditRoles = "System.EditRoles";
        public const string BulletinSettings = "System.BulletinSettings";
        // public const string Settings   = "System.Settings";
        // public const string AttendanceSettings = "System.AttendanceSettings";
        // public const string BehaviourSettings  = "System.BehaviourSettings";
        // public const string FinanceSettings    = "System.FinanceSettings";
        // public const string PersonSettings     = "System.PersonSettings";
        // public const string StaffSettings      = "System.StaffSettings";
        // public const string SenSettings        = "System.SenSettings";
    }

    // Administrative permissions that confer control over the access system itself (user/role/permission
    // management and system configuration). The privilege-escalation ceiling only gates THESE: an actor
    // may freely provision ordinary functional permissions they don't personally hold (e.g. IT support
    // assigning "take the register" to a teacher), but may not grant or assign administrative control
    // beyond their own. Segregation of duties for sensitive *functional* permissions (finance,
    // safeguarding, restricted attendance codes) is a separate concern, not enforced here.
    public static bool IsProtected(string permissionName) =>
        permissionName.StartsWith("System.", StringComparison.OrdinalIgnoreCase);
}
