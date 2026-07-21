SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Next admission number to assign: current max + 1, starting at 1. The UPDLOCK/HOLDLOCK hint takes
-- a range lock so two concurrent create transactions can't both read the same max and collide —
-- the second blocks until the first commits its INSERT. Must be called inside the create
-- transaction (an ambient transaction is passed through by the repository).
CREATE OR ALTER PROCEDURE [dbo].[usp_student_get_next_admission_number]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ISNULL(MAX([AdmissionNumber]), 0) + 1
    FROM [dbo].[Students] WITH (UPDLOCK, HOLDLOCK);
END;
GO
