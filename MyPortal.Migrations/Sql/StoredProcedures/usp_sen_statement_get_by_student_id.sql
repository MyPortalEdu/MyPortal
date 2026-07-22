SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A student's statutory SEN statements / EHC plans and their casework detail.
CREATE OR ALTER PROCEDURE [dbo].[usp_sen_statement_get_by_student_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [StudentId], [IsEhcp], [AssessmentRequestDate], [ParentConsultDate], [FinalisedDate],
        [CeasedDate], [StatutoryAssessmentAgreedId], [StatutoryAssessmentOutcomeId], [SubjectToTribunal],
        [UndergoingMediation], [AppealNotes], [TemporaryDisapplicationSubjects],
        [PermanentDisapplicationSubjects], [LocalAuthorityId], [Comments]
    FROM [dbo].[SenStatements]
    WHERE [StudentId] = @studentId;
END;
