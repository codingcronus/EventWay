SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Events](
	[Ordering] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[EventType] [nvarchar](512) NOT NULL,
	[AggregateType] [nvarchar](512) NOT NULL,
	[AggregateId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[Payload] [nvarchar](max) NOT NULL,
	[MetaData] [nvarchar](max) NULL,
	[Dispatched] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Ordering] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [AK_EventId] UNIQUE NONCLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Events] ADD  DEFAULT ((0)) FOR [Dispatched]
GO

CREATE TABLE [dbo].[ProjectionMetadata](
	[ProjectionType] [nvarchar](200) NOT NULL,
	[ProjectionId] [uniqueidentifier] NOT NULL,
	[EventOffset] [bigint] NOT NULL,
 CONSTRAINT [PKProjectionMetadata] PRIMARY KEY CLUSTERED 
(
	[ProjectionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


