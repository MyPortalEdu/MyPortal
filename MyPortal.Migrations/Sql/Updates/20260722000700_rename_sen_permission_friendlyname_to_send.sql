-- The user-facing label follows the current England statutory term SEND (Special Educational Needs and
-- Disabilities, post-2014 Children & Families Act). Permission Name and Area stay `Sen` (code identifiers);
-- only the catalogue FriendlyName shown in the role-permission admin screen flips to SEND. Guarded on the
-- old value so it's idempotent and won't clobber a locally-customised label.

UPDATE [dbo].[Permissions] SET [FriendlyName] = N'View SEND'
    WHERE [Name] = N'Student.ViewStudentSen' AND [FriendlyName] = N'View SEN';

UPDATE [dbo].[Permissions] SET [FriendlyName] = N'Edit SEND'
    WHERE [Name] = N'Student.EditStudentSen' AND [FriendlyName] = N'Edit SEN';
