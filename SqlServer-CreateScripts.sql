/****** Object:  Table [dbo].[Events]    Script Date: 08-11-2017 14:42:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Events](
	[Ordering] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[EventType] [nvarchar](450) NOT NULL,
	[AggregateType] [nvarchar](100) NOT NULL,
	[AggregateId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[Payload] [nvarchar](max) NOT NULL,
	[MetaData] [nvarchar](max) NULL,
	[Dispatched] [bit] NOT NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[Ordering] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [AK_EventId] UNIQUE NONCLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO

ALTER TABLE [dbo].[Events] ADD  CONSTRAINT [DF__Events__Dispatch__4AB81AF0]  DEFAULT ((0)) FOR [Dispatched]
GO


CREATE TABLE [dbo].[SnapshotEvents](
	[Ordering] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[EventType] [nvarchar](450) NOT NULL,
	[AggregateType] [nvarchar](100) NOT NULL,
	[AggregateId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[Payload] [nvarchar](max) NOT NULL,
	[MetaData] [nvarchar](max) NULL,
	[Dispatched] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Ordering] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [AK_SnapshotEventId] UNIQUE NONCLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[SnapshotEvents] ADD  DEFAULT ((0)) FOR [Dispatched]
GO
