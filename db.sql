USE [master]
GO
/****** Object:  Database [CustomerSupportDB]    Script Date: 6/5/2025 08:34:00 ******/
CREATE DATABASE [CustomerSupportDB]
GO
ALTER DATABASE [CustomerSupportDB] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CustomerSupportDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CustomerSupportDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CustomerSupportDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CustomerSupportDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [CustomerSupportDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CustomerSupportDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET RECOVERY FULL 
GO
ALTER DATABASE [CustomerSupportDB] SET  MULTI_USER 
GO
ALTER DATABASE [CustomerSupportDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CustomerSupportDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [CustomerSupportDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [CustomerSupportDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [CustomerSupportDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'CustomerSupportDB', N'ON'
GO
ALTER DATABASE [CustomerSupportDB] SET QUERY_STORE = OFF
GO
USE [CustomerSupportDB]
GO
/****** Object:  DatabaseRole [db_client]    Script Date: 6/5/2025 08:34:01 ******/
CREATE ROLE [db_client]
GO
/****** Object:  DatabaseRole [db_agent]    Script Date: 6/5/2025 08:34:01 ******/
CREATE ROLE [db_agent]
GO
/****** Object:  DatabaseRole [db_admin]    Script Date: 6/5/2025 08:34:01 ******/
CREATE ROLE [db_admin]
GO
/****** Object:  Schema [admin]    Script Date: 6/5/2025 08:34:01 ******/
CREATE SCHEMA [admin]
GO
/****** Object:  Schema [auth]    Script Date: 6/5/2025 08:34:01 ******/
CREATE SCHEMA [auth]
GO
/****** Object:  Schema [chat]    Script Date: 6/5/2025 08:34:01 ******/
CREATE SCHEMA [chat]
GO
/****** Object:  Schema [crm]    Script Date: 6/5/2025 08:34:01 ******/
CREATE SCHEMA [crm]
GO
/****** Object:  Table [auth].[UsersHistory]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[UsersHistory](
	[UserId] [uniqueidentifier] NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](255) MASKED WITH (FUNCTION = 'email()') NOT NULL,
	[PasswordHash] [varbinary](256) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SecurityStamp] [uniqueidentifier] NOT NULL,
	[ConcurrencyStamp] [uniqueidentifier] NOT NULL,
	[LastLoginAt] [datetime2](7) NULL,
	[FailedLoginAttempts] [int] NOT NULL,
	[LockoutEnd] [datetime2](7) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[RowVersion] [timestamp] NOT NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Index [ix_UsersHistory]    Script Date: 6/5/2025 08:34:01 ******/
CREATE CLUSTERED INDEX [ix_UsersHistory] ON [auth].[UsersHistory]
(
	[ValidTo] ASC,
	[ValidFrom] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Table [auth].[Users]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[Users](
	[UserId] [uniqueidentifier] NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](255) MASKED WITH (FUNCTION = 'email()') NOT NULL,
	[PasswordHash] [varbinary](256) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SecurityStamp] [uniqueidentifier] NOT NULL,
	[ConcurrencyStamp] [uniqueidentifier] NOT NULL,
	[LastLoginAt] [datetime2](7) NULL,
	[FailedLoginAttempts] [int] NOT NULL,
	[LockoutEnd] [datetime2](7) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[RowVersion] [timestamp] NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON ( HISTORY_TABLE = [auth].[UsersHistory] )
)
GO
/****** Object:  Table [auth].[AppRoles]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[AppRoles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_AppRoles] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [auth].[AuthTokens]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[AuthTokens](
	[TokenId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[TokenType] [nvarchar](50) NOT NULL,
	[JwtId] [nvarchar](100) NULL,
	[Token] [nvarchar](500) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ExpiresAt] [datetime2](7) NOT NULL,
	[Revoked] [bit] NOT NULL,
	[Used] [bit] NOT NULL,
	[ReplacedByTokenId] [uniqueidentifier] NULL,
	[IpAddress] [nvarchar](45) NULL,
	[DeviceInfo] [nvarchar](200) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_AuthTokens] PRIMARY KEY CLUSTERED 
(
	[TokenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [auth].[UserRoles]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[UserRoles](
	[UserId] [uniqueidentifier] NOT NULL,
	[RoleId] [int] NOT NULL,
	[AssignedAt] [datetime2](7) NOT NULL,
	[AssignedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [crm].[Contacts]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [crm].[Contacts](
	[ContactId] [uniqueidentifier] NOT NULL,
	[CompanyName] [nvarchar](150) NOT NULL,
	[ContactName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Phone] [nvarchar](20) NULL,
	[Country] [nvarchar](100) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED 
(
	[ContactId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__AppRoles__8A2B6160ED5D1CEF]    Script Date: 6/5/2025 08:34:01 ******/
ALTER TABLE [auth].[AppRoles] ADD UNIQUE NONCLUSTERED 
(
	[RoleName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuthTokens_Active]    Script Date: 6/5/2025 08:34:01 ******/
CREATE NONCLUSTERED INDEX [IX_AuthTokens_Active] ON [auth].[AuthTokens]
(
	[ExpiresAt] ASC
)
WHERE ([Revoked]=(0) AND [Used]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AuthTokens_User_Type]    Script Date: 6/5/2025 08:34:01 ******/
CREATE NONCLUSTERED INDEX [IX_AuthTokens_User_Type] ON [auth].[AuthTokens]
(
	[UserId] ASC,
	[TokenType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__A9D10534BABBE0AD]    Script Date: 6/5/2025 08:34:01 ******/
ALTER TABLE [auth].[Users] ADD UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [auth].[AppRoles] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO
ALTER TABLE [auth].[AppRoles] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT (newid()) FOR [TokenId]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT ((0)) FOR [Revoked]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT ((0)) FOR [Used]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO
ALTER TABLE [auth].[UserRoles] ADD  DEFAULT (sysutcdatetime()) FOR [AssignedAt]
GO
ALTER TABLE [auth].[UserRoles] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [AssignedBy]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT (newid()) FOR [UserId]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT ((0)) FOR [IsActive]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT (newid()) FOR [SecurityStamp]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT (newid()) FOR [ConcurrencyStamp]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT ((0)) FOR [FailedLoginAttempts]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [crm].[Contacts] ADD  DEFAULT (newid()) FOR [ContactId]
GO
ALTER TABLE [crm].[Contacts] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [auth].[AppRoles]  WITH CHECK ADD  CONSTRAINT [FK_AppRoles_CreatedBy_User] FOREIGN KEY([CreatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[AppRoles] CHECK CONSTRAINT [FK_AppRoles_CreatedBy_User]
GO
ALTER TABLE [auth].[AppRoles]  WITH CHECK ADD  CONSTRAINT [FK_AppRoles_UpdatedBy_User] FOREIGN KEY([UpdatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[AppRoles] CHECK CONSTRAINT [FK_AppRoles_UpdatedBy_User]
GO
ALTER TABLE [auth].[AuthTokens]  WITH CHECK ADD  CONSTRAINT [FK_AuthTokens_CreatedBy_User] FOREIGN KEY([CreatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[AuthTokens] CHECK CONSTRAINT [FK_AuthTokens_CreatedBy_User]
GO
ALTER TABLE [auth].[AuthTokens]  WITH CHECK ADD  CONSTRAINT [FK_AuthTokens_ReplacedBy_Token] FOREIGN KEY([ReplacedByTokenId])
REFERENCES [auth].[AuthTokens] ([TokenId])
GO
ALTER TABLE [auth].[AuthTokens] CHECK CONSTRAINT [FK_AuthTokens_ReplacedBy_Token]
GO
ALTER TABLE [auth].[AuthTokens]  WITH CHECK ADD  CONSTRAINT [FK_AuthTokens_Users] FOREIGN KEY([UserId])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[AuthTokens] CHECK CONSTRAINT [FK_AuthTokens_Users]
GO
ALTER TABLE [auth].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_AssignedBy_User] FOREIGN KEY([AssignedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_AssignedBy_User]
GO
ALTER TABLE [auth].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId])
REFERENCES [auth].[AppRoles] ([RoleId])
GO
ALTER TABLE [auth].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Roles]
GO
ALTER TABLE [auth].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserId])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Users]
GO
ALTER TABLE [auth].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_CreatedBy_User] FOREIGN KEY([CreatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[Users] CHECK CONSTRAINT [FK_Users_CreatedBy_User]
GO
ALTER TABLE [auth].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_UpdatedBy_User] FOREIGN KEY([UpdatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[Users] CHECK CONSTRAINT [FK_Users_UpdatedBy_User]
GO
ALTER TABLE [auth].[AuthTokens]  WITH CHECK ADD  CONSTRAINT [CHK_AuthTokens_Type] CHECK  (([TokenType]='PasswordReset' OR [TokenType]='Verification' OR [TokenType]='Refresh'))
GO
ALTER TABLE [auth].[AuthTokens] CHECK CONSTRAINT [CHK_AuthTokens_Type]
GO
USE [master]
GO
ALTER DATABASE [CustomerSupportDB] SET  READ_WRITE 
GO
USE [master]
GO
/****** Object:  Database [CustomerSupportDB]    Script Date: 6/5/2025 08:34:00 ******/
CREATE DATABASE [CustomerSupportDB]
GO
ALTER DATABASE [CustomerSupportDB] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CustomerSupportDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CustomerSupportDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CustomerSupportDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CustomerSupportDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [CustomerSupportDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CustomerSupportDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET RECOVERY FULL 
GO
ALTER DATABASE [CustomerSupportDB] SET  MULTI_USER 
GO
ALTER DATABASE [CustomerSupportDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CustomerSupportDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [CustomerSupportDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [CustomerSupportDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [CustomerSupportDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [CustomerSupportDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'CustomerSupportDB', N'ON'
GO
ALTER DATABASE [CustomerSupportDB] SET QUERY_STORE = OFF
GO
USE [CustomerSupportDB]
GO
/****** Object:  DatabaseRole [db_client]    Script Date: 6/5/2025 08:34:01 ******/
CREATE ROLE [db_client]
GO
/****** Object:  DatabaseRole [db_agent]    Script Date: 6/5/2025 08:34:01 ******/
CREATE ROLE [db_agent]
GO
/****** Object:  DatabaseRole [db_admin]    Script Date: 6/5/2025 08:34:01 ******/
CREATE ROLE [db_admin]
GO
/****** Object:  Schema [admin]    Script Date: 6/5/2025 08:34:01 ******/
CREATE SCHEMA [admin]
GO
/****** Object:  Schema [auth]    Script Date: 6/5/2025 08:34:01 ******/
CREATE SCHEMA [auth]
GO
/****** Object:  Schema [chat]    Script Date: 6/5/2025 08:34:01 ******/
CREATE SCHEMA [chat]
GO
/****** Object:  Schema [crm]    Script Date: 6/5/2025 08:34:01 ******/
CREATE SCHEMA [crm]
GO
/****** Object:  Table [auth].[UsersHistory]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[UsersHistory](
	[UserId] [uniqueidentifier] NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](255) MASKED WITH (FUNCTION = 'email()') NOT NULL,
	[PasswordHash] [varbinary](256) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SecurityStamp] [uniqueidentifier] NOT NULL,
	[ConcurrencyStamp] [uniqueidentifier] NOT NULL,
	[LastLoginAt] [datetime2](7) NULL,
	[FailedLoginAttempts] [int] NOT NULL,
	[LockoutEnd] [datetime2](7) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[RowVersion] [timestamp] NOT NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Index [ix_UsersHistory]    Script Date: 6/5/2025 08:34:01 ******/
CREATE CLUSTERED INDEX [ix_UsersHistory] ON [auth].[UsersHistory]
(
	[ValidTo] ASC,
	[ValidFrom] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Table [auth].[Users]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[Users](
	[UserId] [uniqueidentifier] NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](255) MASKED WITH (FUNCTION = 'email()') NOT NULL,
	[PasswordHash] [varbinary](256) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SecurityStamp] [uniqueidentifier] NOT NULL,
	[ConcurrencyStamp] [uniqueidentifier] NOT NULL,
	[LastLoginAt] [datetime2](7) NULL,
	[FailedLoginAttempts] [int] NOT NULL,
	[LockoutEnd] [datetime2](7) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[RowVersion] [timestamp] NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON ( HISTORY_TABLE = [auth].[UsersHistory] )
)
GO
/****** Object:  Table [auth].[AppRoles]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[AppRoles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_AppRoles] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [auth].[AuthTokens]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[AuthTokens](
	[TokenId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[TokenType] [nvarchar](50) NOT NULL,
	[JwtId] [nvarchar](100) NULL,
	[Token] [nvarchar](500) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ExpiresAt] [datetime2](7) NOT NULL,
	[Revoked] [bit] NOT NULL,
	[Used] [bit] NOT NULL,
	[ReplacedByTokenId] [uniqueidentifier] NULL,
	[IpAddress] [nvarchar](45) NULL,
	[DeviceInfo] [nvarchar](200) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_AuthTokens] PRIMARY KEY CLUSTERED 
(
	[TokenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [auth].[UserRoles]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [auth].[UserRoles](
	[UserId] [uniqueidentifier] NOT NULL,
	[RoleId] [int] NOT NULL,
	[AssignedAt] [datetime2](7) NOT NULL,
	[AssignedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [crm].[Contacts]    Script Date: 6/5/2025 08:34:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [crm].[Contacts](
	[ContactId] [uniqueidentifier] NOT NULL,
	[CompanyName] [nvarchar](150) NOT NULL,
	[ContactName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Phone] [nvarchar](20) NULL,
	[Country] [nvarchar](100) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED 
(
	[ContactId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__AppRoles__8A2B6160ED5D1CEF]    Script Date: 6/5/2025 08:34:01 ******/
ALTER TABLE [auth].[AppRoles] ADD UNIQUE NONCLUSTERED 
(
	[RoleName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuthTokens_Active]    Script Date: 6/5/2025 08:34:01 ******/
CREATE NONCLUSTERED INDEX [IX_AuthTokens_Active] ON [auth].[AuthTokens]
(
	[ExpiresAt] ASC
)
WHERE ([Revoked]=(0) AND [Used]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AuthTokens_User_Type]    Script Date: 6/5/2025 08:34:01 ******/
CREATE NONCLUSTERED INDEX [IX_AuthTokens_User_Type] ON [auth].[AuthTokens]
(
	[UserId] ASC,
	[TokenType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__A9D10534BABBE0AD]    Script Date: 6/5/2025 08:34:01 ******/
ALTER TABLE [auth].[Users] ADD UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [auth].[AppRoles] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO
ALTER TABLE [auth].[AppRoles] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT (newid()) FOR [TokenId]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT ((0)) FOR [Revoked]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT ((0)) FOR [Used]
GO
ALTER TABLE [auth].[AuthTokens] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO
ALTER TABLE [auth].[UserRoles] ADD  DEFAULT (sysutcdatetime()) FOR [AssignedAt]
GO
ALTER TABLE [auth].[UserRoles] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [AssignedBy]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT (newid()) FOR [UserId]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT ((0)) FOR [IsActive]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT (newid()) FOR [SecurityStamp]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT (newid()) FOR [ConcurrencyStamp]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT ((0)) FOR [FailedLoginAttempts]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO
ALTER TABLE [auth].[Users] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [crm].[Contacts] ADD  DEFAULT (newid()) FOR [ContactId]
GO
ALTER TABLE [crm].[Contacts] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [auth].[AppRoles]  WITH CHECK ADD  CONSTRAINT [FK_AppRoles_CreatedBy_User] FOREIGN KEY([CreatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[AppRoles] CHECK CONSTRAINT [FK_AppRoles_CreatedBy_User]
GO
ALTER TABLE [auth].[AppRoles]  WITH CHECK ADD  CONSTRAINT [FK_AppRoles_UpdatedBy_User] FOREIGN KEY([UpdatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[AppRoles] CHECK CONSTRAINT [FK_AppRoles_UpdatedBy_User]
GO
ALTER TABLE [auth].[AuthTokens]  WITH CHECK ADD  CONSTRAINT [FK_AuthTokens_CreatedBy_User] FOREIGN KEY([CreatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[AuthTokens] CHECK CONSTRAINT [FK_AuthTokens_CreatedBy_User]
GO
ALTER TABLE [auth].[AuthTokens]  WITH CHECK ADD  CONSTRAINT [FK_AuthTokens_ReplacedBy_Token] FOREIGN KEY([ReplacedByTokenId])
REFERENCES [auth].[AuthTokens] ([TokenId])
GO
ALTER TABLE [auth].[AuthTokens] CHECK CONSTRAINT [FK_AuthTokens_ReplacedBy_Token]
GO
ALTER TABLE [auth].[AuthTokens]  WITH CHECK ADD  CONSTRAINT [FK_AuthTokens_Users] FOREIGN KEY([UserId])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[AuthTokens] CHECK CONSTRAINT [FK_AuthTokens_Users]
GO
ALTER TABLE [auth].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_AssignedBy_User] FOREIGN KEY([AssignedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_AssignedBy_User]
GO
ALTER TABLE [auth].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId])
REFERENCES [auth].[AppRoles] ([RoleId])
GO
ALTER TABLE [auth].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Roles]
GO
ALTER TABLE [auth].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserId])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Users]
GO
ALTER TABLE [auth].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_CreatedBy_User] FOREIGN KEY([CreatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[Users] CHECK CONSTRAINT [FK_Users_CreatedBy_User]
GO
ALTER TABLE [auth].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_UpdatedBy_User] FOREIGN KEY([UpdatedBy])
REFERENCES [auth].[Users] ([UserId])
GO
ALTER TABLE [auth].[Users] CHECK CONSTRAINT [FK_Users_UpdatedBy_User]
GO
ALTER TABLE [auth].[AuthTokens]  WITH CHECK ADD  CONSTRAINT [CHK_AuthTokens_Type] CHECK  (([TokenType]='PasswordReset' OR [TokenType]='Verification' OR [TokenType]='Refresh'))
GO
ALTER TABLE [auth].[AuthTokens] CHECK CONSTRAINT [CHK_AuthTokens_Type]
GO
USE [master]
GO
ALTER DATABASE [CustomerSupportDB] SET  READ_WRITE 
GO
