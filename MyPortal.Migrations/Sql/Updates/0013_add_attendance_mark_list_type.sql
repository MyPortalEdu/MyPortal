CREATE TYPE [dbo].[AttendanceMarkList] AS TABLE
(
    [StudentId]        UNIQUEIDENTIFIER NOT NULL,
    [AttendanceCodeId] UNIQUEIDENTIFIER NOT NULL,
    [Comments]         NVARCHAR(256)    NULL,
    [MinutesLate]      INT              NULL,
    PRIMARY KEY CLUSTERED ([StudentId])
);
