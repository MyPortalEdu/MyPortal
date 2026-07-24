SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Certificates (self-certifications, fit notes, return-to-work records) for a set of absences.
-- Lean table like its parent — no audit / soft-delete.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_absence_certificate_get_by_absence_ids]
    @absenceIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SAC.[Id], SAC.[StaffAbsenceId], SAC.[DateReceived], SAC.[DateSigned],
        SAC.[IsSelfCertified], SAC.[IsReturnToWork], SAC.[SignedBy], SAC.[Notes]
    FROM [dbo].[StaffAbsenceCertificates] AS SAC
        INNER JOIN @absenceIds AS AI ON AI.[Value] = SAC.[StaffAbsenceId]
    ORDER BY SAC.[DateReceived] DESC;
END;
