/****** Object:  Table [dbo].[Admins]    Script Date: 11/07/2023 15:41:49 ******/

USE DWC
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Admins](
	[Id] [nvarchar](128) NOT NULL,
	[Username] [nvarchar](128) NOT NULL,
	[PasswordHash] [nvarchar](128) NOT NULL,
	CONSTRAINT [PK_dbo.Admins] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


INSERT INTO [dbo].[Admins]
           ([Id]
           ,[Username]
           ,[PasswordHash])
     VALUES
           (NEWID()
           ,'Admin'
           ,'21232f297a57a5a743894a0e4a801fc3')
GO


/****** Object:  Table [dbo].[Locations]    Script Date: 11/07/2023 15:41:49 ******/

USE DWC
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Locations](
	[Id] [nvarchar](128) NOT NULL,
	[Location] [nvarchar](256) NULL,
	CONSTRAINT [PK_dbo.Locations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[Departments]    Script Date: 11/07/2023 15:41:49 ******/
USE DWC
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departments](
	[Id] [nvarchar](128) NOT NULL,
	[Department] [nvarchar](256) NULL,
	CONSTRAINT [PK_dbo.Departments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



/****** Object:  Table [dbo].[Users]    Script Date: 11/07/2023 15:41:49 ******/
USE DWC
G
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Sip] [nvarchar](128) NOT NULL,
	[Number] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](128) NULL,
	[Image] [nvarchar](256) NULL,
	[DepartmentId] [nvarchar](128) NOT NULL,
	[LocationId] [nvarchar](128) NOT NULL
	CONSTRAINT [PK_dbo.Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
USE DWC
GO


-- Criar a restrição de chave estrangeira para a coluna DepartmentId
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_Users_Departments] FOREIGN KEY ([DepartmentId])
REFERENCES [dbo].[Departments] ([Id])

-- Criar a restrição de chave estrangeira para a coluna LocationId
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_Users_Locations] FOREIGN KEY ([LocationId])
REFERENCES [dbo].[Locations] ([Id])

/****** Object:  Table [dbo].[Users]    Script Date: 11/07/2023 15:41:49 ******/
USE DWC
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[UserId] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Sip] [nvarchar](128) NOT NULL,
	[Number] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](128) NULL,
	[Image] [nvarchar](256) NULL,
	[DepartmentId] [nvarchar](128) NOT NULL,
	[LocationId] [nvarchar](128) NOT NULL
	CONSTRAINT [PK_dbo.Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Status]    Script Date: 13/07/2023 16:20:58 ******/
USE [DWC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Status](
	[Id] [nvarchar](128) NOT NULL,
	[StatusName] [nvarchar](128) NOT NULL,
	[Color] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.Status] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Insert:  Table [dbo].[Locations]    Script Date: 11/07/2023 15:41:49 ******/
INSERT INTO [dbo].[Locations]
           ([Id]
           ,[Location])
     VALUES
           (NEWID()
           ,'Florianópolis-SC')
GO

/****** Insert:  Table [dbo].[Departments]    Script Date: 11/07/2023 15:41:49 ******/
INSERT INTO [dbo].[Departments]
           ([Id]
           ,[Department])
     VALUES
           (NEWID()
           ,'Desenvolvimento')
GO


/****** Insert:  Table [dbo].[Users]    Script Date: 11/07/2023 15:41:49 ******/
USE [DWC]
GO
INSERT INTO [dbo].[Users]
           ([UserId]
           ,[Name]
           ,[Sip]
           ,[Number]
           ,[Email]
           ,[Image]
           ,[DepartmentId]
           ,[LocationId])
     VALUES
           (NEWID()
           ,'Danilo Volz'
           ,'danilo'
           ,'4101'
           ,'danilo@wecom.com.br'
           ,'./images/danilo-user.jpg'
           ,(SELECT [Id] FROM [dbo].[Departments] WHERE [Department] = 'Suporte Técnico'),
        (SELECT [Id] FROM [dbo].[Locations] WHERE [Location] = 'Porto Alegre-RS'))
GO

/****** Delete:  Table [dbo].[Users]    Script Date: 11/07/2023 15:41:49 ******/

DELETE FROM [dbo].[Users] WHERE Sip == sip
