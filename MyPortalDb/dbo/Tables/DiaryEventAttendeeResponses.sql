﻿CREATE TABLE [dbo].[DiaryEventAttendeeResponses] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ClusterId]   INT              IDENTITY (1, 1) NOT NULL,
    [Description] NVARCHAR (256)   NOT NULL,
    [Active]      BIT              NOT NULL,
    CONSTRAINT [PK_DiaryEventAttendeeResponses] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE CLUSTERED INDEX [CIX_ClusterId]
    ON [dbo].[DiaryEventAttendeeResponses]([ClusterId] ASC);

