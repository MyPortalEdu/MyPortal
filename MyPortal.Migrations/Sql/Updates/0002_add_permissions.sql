MERGE INTO [dbo].[Permissions] AS Target
    USING (VALUES
    -- Admissions
    (N'Admissions.ViewApplications',     N'View Applications',        N'Admissions'),
    (N'Admissions.EditApplications',     N'Edit Applications',        N'Admissions'),
    (N'Admissions.ViewEnquiries',        N'View Enquiries',           N'Admissions'),
    (N'Admissions.EditEnquiries',        N'Edit Enquiries',           N'Admissions'),
    (N'Admissions.ViewInterviews',       N'View Interviews',          N'Admissions'),
    (N'Admissions.EditInterviews',       N'Edit Interviews',          N'Admissions'),

    -- Agencies
    (N'Agencies.ViewAgencies',           N'View Agencies',            N'Agencies'),
    (N'Agencies.EditAgencies',           N'Edit Agencies',            N'Agencies'),

    -- Assessment
    (N'Assessment.ViewAspects',          N'View Aspects',             N'Assessment'),
    (N'Assessment.EditAspects',          N'Edit Aspects',             N'Assessment'),
    (N'Assessment.ViewExamBaseData',     N'View Exam Base Data',      N'Assessment'),
    (N'Assessment.EditExamBaseData',     N'Edit Exam Base Data',      N'Assessment'),
    (N'Assessment.RunExamAsst',          N'Run Exam Assistant',       N'Assessment'),
    (N'Assessment.ViewGradeSets',        N'View Grade Sets',          N'Assessment'),
    (N'Assessment.EditGradeSets',        N'Edit Grade Sets',          N'Assessment'),
    (N'Assessment.ViewMarksheetTemplates', N'View Marksheet Templates', N'Assessment'),
    (N'Assessment.EditMarksheetTemplates', N'Edit Marksheet Templates', N'Assessment'),
    (N'Assessment.ViewResultSets',       N'View Result Sets',         N'Assessment'),
    (N'Assessment.EditResultSets',       N'Edit Result Sets',         N'Assessment'),
    (N'Assessment.ViewOwnMarksheets',    N'View Own Marksheets',      N'Assessment'),
    (N'Assessment.ViewAllMarksheets',    N'View All Marksheets',      N'Assessment'),
    (N'Assessment.UpdateOwnMarksheets',  N'Update Own Marksheets',    N'Assessment'),
    (N'Assessment.UpdateAllMarksheets',  N'Update All Marksheets',    N'Assessment'),
    (N'Assessment.ViewResults',          N'View Results',             N'Assessment'),
    (N'Assessment.ViewEmbargoedResults', N'View Embargoed Results',   N'Assessment'),
    (N'Assessment.EditResults',          N'Edit Results',             N'Assessment'),

    -- Attendance
    (N'Attendance.ViewAttendanceMarks',  N'View Attendance Marks',    N'Attendance'),
    (N'Attendance.EditAttendanceMarks',  N'Edit Attendance Marks',    N'Attendance'),
    (N'Attendance.UseRestrictedCodes',   N'Use Restricted Codes',     N'Attendance'),

    -- Behaviour
    (N'Behaviour.ViewAchievements',      N'View Achievements',        N'Behaviour'),
    (N'Behaviour.EditAchievements',      N'Edit Achievements',        N'Behaviour'),
    (N'Behaviour.ViewIncidents',         N'View Incidents',           N'Behaviour'),
    (N'Behaviour.EditIncidents',         N'Edit Incidents',           N'Behaviour'),
    (N'Behaviour.ViewDetentions',        N'View Detentions',          N'Behaviour'),
    (N'Behaviour.EditDetentions',        N'Edit Detentions',          N'Behaviour'),
    (N'Behaviour.ViewExclusions',        N'View Exclusions',          N'Behaviour'),
    (N'Behaviour.EditExclusions',        N'Edit Exclusions',          N'Behaviour'),
    (N'Behaviour.ViewReportCards',       N'View Report Cards',        N'Behaviour'),
    (N'Behaviour.EditReportCards',       N'Edit Report Cards',        N'Behaviour'),
    (N'Behaviour.AddRemoveReportCards',  N'Add/Remove Report Cards',  N'Behaviour'),

    -- Curriculum
    (N'Curriculum.AcademicStructure',    N'Academic Structure',       N'Curriculum'),
    (N'Curriculum.EditAcademicYears',    N'Edit Academic Years',      N'Curriculum'),
    (N'Curriculum.ArrangeCover',         N'Arrange Cover',            N'Curriculum'),
    (N'Curriculum.ViewHomework',         N'View Homework',            N'Curriculum'),
    (N'Curriculum.EditHomework',         N'Edit Homework',            N'Curriculum'),
    (N'Curriculum.ViewLessonPlans',      N'View Lesson Plans',        N'Curriculum'),
    (N'Curriculum.EditLessonPlans',      N'Edit Lesson Plans',        N'Curriculum'),
    (N'Curriculum.ViewStudyTopics',      N'View Study Topics',        N'Curriculum'),
    (N'Curriculum.EditStudyTopics',      N'Edit Study Topics',        N'Curriculum'),

    -- Finance
    (N'Finance.ViewAccounts',            N'View Accounts',            N'Finance'),
    (N'Finance.EditAccounts',            N'Edit Accounts',            N'Finance'),
    (N'Finance.ViewProducts',            N'View Products',            N'Finance'),
    (N'Finance.EditProducts',            N'Edit Products',            N'Finance'),
    (N'Finance.ViewBills',               N'View Bills',               N'Finance'),
    (N'Finance.EditBills',               N'Edit Bills',               N'Finance'),
    (N'Finance.ViewCharges',             N'View Charges',             N'Finance'),
    (N'Finance.EditCharges',             N'Edit Charges',             N'Finance'),
    (N'Finance.ViewDiscounts',           N'View Discounts',           N'Finance'),
    (N'Finance.EditDiscounts',           N'Edit Discounts',           N'Finance'),

    -- People
    (N'People.ViewAgentDetails',                 N'View Agent Details',                    N'People'),
    (N'People.EditAgentDetails',                 N'Edit Agent Details',                    N'People'),
    (N'People.ViewContactDetails',               N'View Contact Details',                  N'People'),
    (N'People.EditContactDetails',               N'Edit Contact Details',                  N'People'),
    (N'People.ViewContactTasks',                 N'View Contact Tasks',                    N'People'),
    (N'People.EditContactTasks',                 N'Edit Contact Tasks',                    N'People'),

    -- Profiles
    (N'Profiles.ViewCommentBanks',      N'View Comment Banks',        N'Profiles'),
    (N'Profiles.EditCommentBanks',      N'Edit Comment Banks',        N'Profiles'),
    (N'Profiles.ViewReports',           N'View Reports',              N'Profiles'),
    (N'Profiles.EditOwnReports',        N'Edit Own Reports',          N'Profiles'),
    (N'Profiles.EditAllReports',        N'Edit All Reports',          N'Profiles'),
    (N'Profiles.ViewReportingSessions', N'View Reporting Sessions',   N'Profiles'),
    (N'Profiles.EditReportingSessions', N'Edit Reporting Sessions',   N'Profiles'),

    -- School
    (N'School.PastoralStructure',       N'Pastoral Structure',        N'School'),
    (N'School.ViewRooms',               N'View Rooms',                N'School'),
    (N'School.EditRooms',               N'Edit Rooms',                N'School'),
    (N'School.ViewSchoolDetails',       N'View School Details',       N'School'),
    (N'School.EditSchoolDetails',       N'Edit School Details',       N'School'),
    (N'School.ViewSchoolDiary',         N'View School Diary',         N'School'),
    (N'School.EditSchoolDiary',         N'Edit School Diary',         N'School'),
    (N'School.ViewSchoolDocuments',     N'View School Documents',     N'School'),
    (N'School.EditSchoolDocuments',     N'Edit School Documents',     N'School'),
    (N'School.ViewSchoolBulletins',     N'View School Bulletins',     N'School'),
    (N'School.EditSchoolBulletins',     N'Edit School Bulletins',     N'School'),
    (N'School.ApproveSchoolBulletins',  N'Approve School Bulletins',  N'School'),
    
    -- Staff
    (N'Staff.ViewStaffBasicDetails',            N'View Staff Basic Details',              N'Staff'),
    (N'Staff.ViewStaffEmploymentDetails',       N'View Staff Employment Details',         N'Staff'),
    (N'Staff.EditStaffBasicDetails',            N'Edit Staff Basic Details',              N'Staff'),
    (N'Staff.EditStaffEmploymentDetails',       N'Edit Staff Employment Details',         N'Staff'),
    (N'Staff.ViewAllStaffDocuments',            N'View All Staff Documents',              N'Staff'),
    (N'Staff.ViewManagedStaffDocuments',        N'View Managed Staff Documents',          N'Staff'),
    (N'Staff.ViewOwnStaffDocuments',            N'View Own Staff Documents',              N'Staff'),
    (N'Staff.EditAllStaffDocuments',            N'Edit All Staff Documents',              N'Staff'),
    (N'Staff.EditManagedStaffDocuments',        N'Edit Managed Staff Documents',          N'Staff'),
    (N'Staff.EditOwnStaffDocuments',            N'Edit Own Staff Documents',              N'Staff'),
    (N'Staff.ViewAllStaffPerformanceDetails',   N'View All Staff Performance Details',    N'Staff'),
    (N'Staff.ViewManagedStaffPerformanceDetails', N'View Managed Staff Performance Details', N'Staff'),
    (N'Staff.ViewOwnStaffPerformanceDetails',   N'View Own Staff Performance Details',    N'Staff'),
    (N'Staff.EditAllStaffPerformanceDetails',   N'Edit All Staff Performance Details',    N'Staff'),
    (N'Staff.EditManagedStaffPerformanceDetails', N'Edit Managed Staff Performance Details', N'Staff'),
    (N'Staff.EditOwnStaffPerformanceDetails',   N'Edit Own Staff Performance Details',    N'Staff'),
    (N'Staff.ViewAllStaffTasks',                N'View All Staff Tasks',                  N'Staff'),
    (N'Staff.ViewManagedStaffTasks',            N'View Managed Staff Tasks',              N'Staff'),
    (N'Staff.EditAllStaffTasks',                N'Edit All Staff Tasks',                  N'Staff'),
    (N'Staff.EditManagedStaffTasks',            N'Edit Managed Staff Tasks',              N'Staff'),
    (N'Staff.ViewTrainingCourses',              N'View Training Courses',                 N'Staff'),
    (N'Staff.EditTrainingCourses',              N'Edit Training Courses',                 N'Staff'),

    -- Student
    (N'Student.ViewSenDetails',         N'View SEN Details',          N'Student'),
    (N'Student.EditSenDetails',         N'Edit SEN Details',          N'Student'),
    (N'Student.ViewStudentDetails',     N'View Student Details',      N'Student'),
    (N'Student.EditStudentDetails',     N'Edit Student Details',      N'Student'),
    (N'Student.ViewStudentLogNotes',    N'View Student Log Notes',    N'Student'),
    (N'Student.EditStudentLogNotes',    N'Edit Student Log Notes',    N'Student'),
    (N'Student.ViewStudentTasks',       N'View Student Tasks',        N'Student'),
    (N'Student.EditStudentTasks',       N'Edit Student Tasks',        N'Student'),
    (N'Student.ViewStudentDocuments',   N'View Student Documents',    N'Student'),
    (N'Student.EditStudentDocuments',   N'Edit Student Documents',    N'Student'),
    (N'Student.ViewMedicalEvents',      N'View Medical Events',       N'Student'),
    (N'Student.EditMedicalEvents',      N'Edit Medical Events',       N'Student'),
    (N'Student.ViewFinanceDetails',     N'View Finance Details',      N'Student'),
    (N'Student.EditFinanceDetails',     N'Edit Finance Details',      N'Student'),

    -- System
    (N'System.ViewUsers',               N'View Users',                N'System'),
    (N'System.EditUsers',               N'Edit Users',                N'System'),
    (N'System.ViewGroups',              N'View Groups',               N'System'),
    (N'System.EditGroups',              N'Edit Groups',               N'System'),
    (N'System.Settings',                N'System Settings',           N'System'),
    (N'System.AttendanceSettings',      N'Attendance Settings',       N'System'),
    (N'System.BehaviourSettings',       N'Behaviour Settings',        N'System'),
    (N'System.FinanceSettings',         N'Finance Settings',          N'System'),
    (N'System.PersonSettings',          N'Person Settings',           N'System'),
    (N'System.StaffSettings',           N'Staff Settings',            N'System'),
    (N'System.SenSettings',             N'SEN Settings',              N'System')
    ) AS Source ([Name], [FriendlyName], [Area])
    ON Target.[Name] = Source.[Name]

    WHEN MATCHED AND (Target.[FriendlyName] <> Source.[FriendlyName] OR Target.[Area] <> Source.[Area])
    THEN UPDATE SET [FriendlyName] = Source.[FriendlyName],
             [Area] = Source.[Area]

             WHEN NOT MATCHED BY TARGET
             THEN INSERT ([Id], [Name], [FriendlyName], [Area])
         VALUES (NEWID(), Source.[Name], Source.[FriendlyName], Source.[Area]);
