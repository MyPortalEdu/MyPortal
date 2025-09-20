SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- SQL Server 2017+ (uses CONCAT_WS)
CREATE OR ALTER FUNCTION [dbo].[fn_person_get_name](
    @PersonId UNIQUEIDENTIFIER,
    @NameFormat INT,          -- 1 FullName, 2 FullNameAbbrev, 3 FullNameNoTitle, 4 Initials, else "Last, First [Middle]"
    @UsePreferredName BIT,
    @IncludeMiddleName BIT
)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
WITH Src AS
(
    SELECT
        P.Id,
        Title       = NULLIF(P.Title, ''),
        FirstRaw    = CASE WHEN @UsePreferredName = 1
                           THEN COALESCE(NULLIF(P.PreferredFirstName, ''), NULLIF(P.FirstName, ''))
                           ELSE NULLIF(P.FirstName, '')
                      END,
        LastRaw     = CASE WHEN @UsePreferredName = 1
                           THEN COALESCE(NULLIF(P.PreferredLastName, ''), NULLIF(P.LastName, ''))
                           ELSE NULLIF(P.LastName, '')
                      END,
        MiddleRaw   = CASE WHEN @IncludeMiddleName = 1
                           THEN NULLIF(P.MiddleName, '')
                           ELSE NULL
                      END
    FROM dbo.People AS P
    WHERE P.Id = @PersonId
),
Norm AS
(
    SELECT
        Id,
        Title,
        [First]          = FirstRaw,
        [Last]           = LastRaw,
        [Middle]         = MiddleRaw,
        FirstInitial     = CASE WHEN FirstRaw  IS NOT NULL THEN LEFT(FirstRaw, 1)  END,
        MiddleInitial    = CASE WHEN MiddleRaw IS NOT NULL THEN LEFT(MiddleRaw, 1) END,
        LastInitial      = CASE WHEN LastRaw   IS NOT NULL THEN LEFT(LastRaw, 1)   END
    FROM Src
)
SELECT
    Id,
    [Name] =
    CASE @NameFormat
    WHEN 1 THEN
    -- Title First [Middle] Last
    CONCAT_WS(' ',
    Title,
    [First],
    CASE WHEN [Middle] IS NOT NULL THEN [Middle] END,
    [Last])

    WHEN 2 THEN
    -- Title F [M] Last (abbreviated)
    CONCAT_WS(' ',
    Title,
    FirstInitial,
    CASE WHEN MiddleInitial IS NOT NULL THEN MiddleInitial END,
    [Last])

    WHEN 3 THEN
    -- First [Middle] Last (no title)
    CONCAT_WS(' ',
    [First],
    CASE WHEN [Middle] IS NOT NULL THEN [Middle] END,
    [Last])

    WHEN 4 THEN
    -- Initials: F[M]L (no spaces)
    CONCAT_WS('',
    FirstInitial,
    CASE WHEN MiddleInitial IS NOT NULL THEN MiddleInitial END,
    LastInitial)

    ELSE
    -- Last, First [Middle]
    CONCAT(
    COALESCE([Last], ''),
    CASE WHEN [Last] IS NOT NULL AND ([First] IS NOT NULL OR [Middle] IS NOT NULL) THEN ', ' ELSE '' END,
    COALESCE([First], ''),
    CASE WHEN [Middle] IS NOT NULL THEN CONCAT(' ', [Middle]) ELSE '' END
    )
END
FROM Norm;
GO
