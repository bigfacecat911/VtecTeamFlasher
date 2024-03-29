IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
DROP TABLE [dbo].[Users]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](255) NOT NULL,
	[Lastname] [nvarchar](255) NOT NULL,
	[MiddleName] [nvarchar](255) NULL,
	[Enabled] [bit] NOT NULL,
	[UserType] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](255) NULL,
	[Login] [nvarchar](255) NOT NULL,
	[City] [nvarchar](255) NULL,
	[Phone] [nvarchar](50) NULL,
	[Skype] [nvarchar](100) NULL,
	[VK] [nvarchar](255) NULL,
	[Viber] [bit] NULL,
	[WhatsApp] [bit] NULL,
	[OpenModules] [nvarchar](max) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


INSERT INTO [dbo].[Users]
           ([FirstName]
           ,[Lastname]
           ,[MiddleName]
           ,[Enabled]
           ,[UserType]
           ,[PasswordHash]
           ,[Login])
     VALUES
           ('Admin'
           ,'Adminov'
           ,'Adminovich'
           ,1
           ,'superadmin'
           ,'dSPGKr23Yoxana2Pl9jYxcBA7eNlNeUxqKN0i2yufgA='
           ,'admin')
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Review]') AND type in (N'U'))
DROP TABLE [dbo].[Review]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Review](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NULL,
	[SourceUrl] [nvarchar](150) NULL,
	[UserReview] [nvarchar](max) NOT NULL,
	[ReviewDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Review] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReflashHistory_Review]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReflashHistory]'))
ALTER TABLE [dbo].[ReflashHistory] DROP CONSTRAINT [FK_ReflashHistory_Review]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReflashHistory_Users]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReflashHistory]'))
ALTER TABLE [dbo].[ReflashHistory] DROP CONSTRAINT [FK_ReflashHistory_Users]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReflashHistory]') AND type in (N'U'))
DROP TABLE [dbo].[ReflashHistory]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReflashHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[BinaryFileName] [nvarchar](255) NOT NULL,
	[CarVin] [nvarchar](255) NOT NULL,
	[PreviousBinaryName] [nvarchar](255) NULL,
	[Status] [int] NOT NULL,
	[ReflashDate] [datetime] NOT NULL,
	[Price] [nvarchar](100) NULL,
	[ReflashReviewId] [int] NULL,
 CONSTRAINT [PK_ReflashHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ReflashHistory]  WITH CHECK ADD  CONSTRAINT [FK_ReflashHistory_Review] FOREIGN KEY([ReflashReviewId])
REFERENCES [dbo].[Review] ([Id])
GO

ALTER TABLE [dbo].[ReflashHistory] CHECK CONSTRAINT [FK_ReflashHistory_Review]
GO

ALTER TABLE [dbo].[ReflashHistory]  WITH CHECK ADD  CONSTRAINT [FK_ReflashHistory_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[ReflashHistory] CHECK CONSTRAINT [FK_ReflashHistory_Users]
GO


IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Requests_Users]') AND parent_object_id = OBJECT_ID(N'[dbo].[Requests]'))
ALTER TABLE [dbo].[Requests] DROP CONSTRAINT [FK_Requests_Users]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Requests]') AND type in (N'U'))
DROP TABLE [dbo].[Requests]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Requests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StockBinary] [varbinary](max) NULL,
	[EcuNumber] [nvarchar](150) NULL,
	[BinaryNumber] [nvarchar](150) NULL,
	[EcuPhoto] [varbinary](max) NULL,
	[EcuPhotoFilename] [nvarchar](200) NULL,
	[CarDescription] [nvarchar](400) NOT NULL,
	[Status] [int] NULL,
	[UserId] [int] NOT NULL,
	[RequestDate] [datetime] NULL,
	[ExpectedResolveDate] [datetime] NULL,
	[RequestDetails] [nvarchar](255) NULL,
	[StockBinaryName] [nvarchar](255) NULL,
 CONSTRAINT [PK_Requests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Requests]  WITH CHECK ADD  CONSTRAINT [FK_Requests_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Requests] CHECK CONSTRAINT [FK_Requests_Users]
GO





IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Comments_Requests]') AND parent_object_id = OBJECT_ID(N'[dbo].[Comments]'))
ALTER TABLE [dbo].[Comments] DROP CONSTRAINT [FK_Comments_Requests]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Comments_Users]') AND parent_object_id = OBJECT_ID(N'[dbo].[Comments]'))
ALTER TABLE [dbo].[Comments] DROP CONSTRAINT [FK_Comments_Users]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Comments]') AND type in (N'U'))
DROP TABLE [dbo].[Comments]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Comments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[CommentDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[CommentText] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_Requests] FOREIGN KEY([RequestId])
REFERENCES [dbo].[Requests] ([Id])
GO

ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_Comments_Requests]
GO

ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_Comments_Users]
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[News]') AND type in (N'U'))
DROP TABLE [dbo].[News]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[News](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Caption] [nvarchar](100) NOT NULL,
	[Text] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_News] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Token_Users]') AND parent_object_id = OBJECT_ID(N'[dbo].[Token]'))
ALTER TABLE [dbo].[Token] DROP CONSTRAINT [FK_Token_Users]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Token]') AND type in (N'U'))
DROP TABLE [dbo].[Token]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Token](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Token] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [Unique_Token] UNIQUE NONCLUSTERED 
(
	[Token] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Token]  WITH CHECK ADD  CONSTRAINT [FK_Token_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Token] CHECK CONSTRAINT [FK_Token_Users]
GO



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CarManufacturer]') AND type in (N'U'))
DROP TABLE [dbo].[CarManufacturer]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CarManufacturer](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manufacturer] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_CarManufacturer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO [dbo].[CarManufacturer]
           ([Manufacturer])
     VALUES
           ('Honda'),
           ('Ford'),
           ('Mazda'),
           ('KIA'),
           ('Hyundai')
GO


IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReflashStorage_CarManufacturer]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReflashStorage]'))
ALTER TABLE [dbo].[ReflashStorage] DROP CONSTRAINT [FK_ReflashStorage_CarManufacturer]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReflashStorage_Users]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReflashStorage]'))
ALTER TABLE [dbo].[ReflashStorage] DROP CONSTRAINT [FK_ReflashStorage_Users]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReflashStorage]') AND type in (N'U'))
DROP TABLE [dbo].[ReflashStorage]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ReflashStorage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CarManufacturerId] [int] NOT NULL,
	[Model] [nvarchar](100) NOT NULL,
	[YearOfRelease] [datetime] NOT NULL,
	[TransmissionType] [nvarchar](50) NOT NULL,
	[EcuBinaryNumber] [nvarchar](100) NOT NULL,
	[AltEcuCode] [nvarchar](300) NULL,
	[ReflashFileName] [nvarchar](250) NOT NULL,
	[ReflashBinary] [varbinary](max) NOT NULL,
	[Description] [xml] NULL,
	[DateOfLoad] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_ReflashStorage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[ReflashStorage]  WITH CHECK ADD  CONSTRAINT [FK_ReflashStorage_CarManufacturer] FOREIGN KEY([CarManufacturerId])
REFERENCES [dbo].[CarManufacturer] ([Id])
GO

ALTER TABLE [dbo].[ReflashStorage] CHECK CONSTRAINT [FK_ReflashStorage_CarManufacturer]
GO

ALTER TABLE [dbo].[ReflashStorage]  WITH CHECK ADD  CONSTRAINT [FK_ReflashStorage_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[ReflashStorage] CHECK CONSTRAINT [FK_ReflashStorage_Users]
GO

