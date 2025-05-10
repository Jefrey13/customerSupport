CREATE DATABASE [CustomerSupportDB];
GO

USE [CustomerSupportDB];
GO

/****** Database roles ******/
CREATE ROLE [db_client];
GO
CREATE ROLE [db_agent];
GO
CREATE ROLE [db_admin];
GO

/****** Schemas ******/
CREATE SCHEMA [admin];
GO
CREATE SCHEMA [auth];
GO
CREATE SCHEMA [chat];
GO
CREATE SCHEMA [crm];
GO

/****** Table in crm schema ******/
-- Companies
CREATE TABLE [crm].[Companies](
    [CompanyId] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(150) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED([CompanyId])
) ON [PRIMARY];
GO

/****** Tables in auth schema ******/
-- 1) UsersHistory (history table)
CREATE TABLE [auth].[UsersHistory](
    [UserId] INT NOT NULL,
    [FullName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(255) MASKED WITH (FUNCTION = 'email()') NULL,
    [PasswordHash] VARBINARY(256) NULL,
    [IsActive] BIT NOT NULL,
    [SecurityStamp] UNIQUEIDENTIFIER NOT NULL,
    [ConcurrencyStamp] UNIQUEIDENTIFIER NOT NULL,
    [CompanyId] INT NULL,
    [Phone] NVARCHAR(20) NULL,
    [Identifier] NVARCHAR(50) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL,
    [UpdatedAt] DATETIME2(7) NULL,
    [RowVersion] TIMESTAMP NOT NULL,
    [ValidFrom] DATETIME2(7) NOT NULL,
    [ValidTo] DATETIME2(7) NOT NULL
) ON [PRIMARY];
GO
CREATE CLUSTERED INDEX [ix_UsersHistory] ON [auth].[UsersHistory]([ValidTo],[ValidFrom]);
GO

-- 2) Users
CREATE TABLE [auth].[Users](
    [UserId] INT IDENTITY(1,1) NOT NULL,
    [FullName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(255) MASKED WITH (FUNCTION = 'email()') NULL,
    [PasswordHash] VARBINARY(256) NULL,
    [IsActive] BIT NOT NULL DEFAULT (0),
    [SecurityStamp] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [ConcurrencyStamp] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [CompanyId] INT NULL,
    [Phone] NVARCHAR(20) NULL,
    [Identifier] NVARCHAR(50) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt] DATETIME2(7) NULL,
    [RowVersion] TIMESTAMP NOT NULL,
    [ValidFrom] DATETIME2(7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] DATETIME2(7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED([UserId]),
    PERIOD FOR SYSTEM_TIME ([ValidFrom],[ValidTo])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [auth].[UsersHistory]));
GO
ALTER TABLE [auth].[Users]
    ADD CONSTRAINT FK_Users_Companies FOREIGN KEY([CompanyId]) REFERENCES [crm].[Companies]([CompanyId]);
GO

-- 3) AppRoles
CREATE TABLE [auth].[AppRoles](
    [RoleId] INT IDENTITY(1,1) NOT NULL,
    [RoleName] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(200) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt] DATETIME2(7) NULL,
    [RowVersion] TIMESTAMP NOT NULL,
    CONSTRAINT [PK_AppRoles] PRIMARY KEY CLUSTERED([RoleId])
) ON [PRIMARY];
GO
ALTER TABLE [auth].[AppRoles] ADD CONSTRAINT UQ_AppRoles_RoleName UNIQUE([RoleName]);
GO

-- 4) AuthTokens
CREATE TABLE [auth].[AuthTokens](
    [TokenId] INT IDENTITY(1,1) NOT NULL,
    [UserId] INT NOT NULL,
    [TokenType] NVARCHAR(50) NOT NULL,
    [Token] NVARCHAR(500) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [ExpiresAt] DATETIME2(7) NOT NULL,
    [Revoked] BIT NOT NULL DEFAULT (0),
    [Used] BIT NOT NULL DEFAULT (0),
    [RowVersion] TIMESTAMP NOT NULL,
    CONSTRAINT [PK_AuthTokens] PRIMARY KEY CLUSTERED([TokenId])
) ON [PRIMARY];
GO
ALTER TABLE [auth].[AuthTokens]
    ADD CONSTRAINT FK_AuthTokens_Users FOREIGN KEY([UserId]) REFERENCES [auth].[Users]([UserId]);
GO

-- 5) UserRoles
CREATE TABLE [auth].[UserRoles](
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    [AssignedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED([UserId],[RoleId])
) ON [PRIMARY];
GO
ALTER TABLE [auth].[UserRoles] ADD CONSTRAINT FK_UserRoles_Roles FOREIGN KEY([RoleId]) REFERENCES [auth].[AppRoles]([RoleId]);
GO
ALTER TABLE [auth].[UserRoles] ADD CONSTRAINT FK_UserRoles_Users FOREIGN KEY([UserId]) REFERENCES [auth].[Users]([UserId]);
GO

/****** Tables in chat schema ******/
-- Conversations
CREATE TABLE [chat].[Conversations](
    [ConversationId] INT IDENTITY(1,1) PRIMARY KEY,
    [CompanyId] INT NULL,
    [ClientUserId] INT NULL,
    [AssignedAgent] INT NULL,
    [AssignedBy] INT NULL,
    [AssignedAt] DATETIME2(7) NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Bot',
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt] DATETIME2(7) NULL,
    CONSTRAINT FK_Conversations_Companies FOREIGN KEY([CompanyId]) REFERENCES [crm].[Companies]([CompanyId]),
    CONSTRAINT FK_Conversations_Client FOREIGN KEY([ClientUserId]) REFERENCES [auth].[Users]([UserId]),
    CONSTRAINT FK_Conversations_Agent FOREIGN KEY([AssignedAgent]) REFERENCES [auth].[Users]([UserId]),
    CONSTRAINT FK_Conversations_AssignedBy FOREIGN KEY([AssignedBy]) REFERENCES [auth].[Users]([UserId])
) ON [PRIMARY];
GO

-- Messages
CREATE TABLE [chat].[Messages](
    [MessageId] INT IDENTITY(1,1) PRIMARY KEY,
    [ConversationId] INT NOT NULL,
    [SenderId] INT NOT NULL,
    [Content] NVARCHAR(MAX) NULL,
    [MessageType] NVARCHAR(20) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Messages_Conversations FOREIGN KEY([ConversationId]) REFERENCES [chat].[Conversations]([ConversationId]),
    CONSTRAINT FK_Messages_Sender FOREIGN KEY([SenderId]) REFERENCES [auth].[Users]([UserId])
) ON [PRIMARY];
GO

-- Attachments
CREATE TABLE [chat].[Attachments](
    [AttachmentId] INT IDENTITY(1,1) PRIMARY KEY,
    [MessageId] INT NOT NULL,
    [MediaId] NVARCHAR(100) NOT NULL,
    [FileName] NVARCHAR(200) NULL,
    [MediaUrl] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Attachments_Messages FOREIGN KEY([MessageId]) REFERENCES [chat].[Messages]([MessageId])
) ON [PRIMARY];
GO


ALTER TABLE chat.Attachments
ADD MimeType NVARCHAR(100) NULL;



-- =================================================================================
-- Seed data: Companies, Roles and Admin User
-- =================================================================================

-- 1) Insert 6 companies (including PC Group S.A)
INSERT INTO crm.Companies (Name) VALUES
  ('PC Group S.A'),
  ('Distribuidora La Esquina'),
  ('Panadería El Trigal'),
  ('Farmacia Vida Sana'),
  ('Hotel Paraíso Colonial'),
  ('Librería El Saber');
GO

-- 2) Insert AppRoles (if not exists)
INSERT INTO auth.AppRoles (RoleName, Description) VALUES
  ('Admin',    'System administrator with full access'),
  ('Customer', 'Client using the platform for service management'),
  ('Support',  'Customer support personnel in charge of communication');
GO

-- 3) Insert admin user for PC Group S.A
DECLARE
  @AdminUserId INT,
  @PCGroupId   INT;

-- Assume 'PC Group S.A' entry exists in crm.Companies
SELECT @PCGroupId = CompanyId
  FROM crm.Companies
 WHERE Name = 'PC Group S.A';

INSERT INTO auth.Users (FullName, Email, PasswordHash, IsActive, CompanyId)
VALUES (
  'John Administrator',
  'diviv78453@jazipo.com',
  CONVERT(VARBINARY(256), '$2a$12$t5dMcFCPdvt.EBXvdb1Kee8tNnVUmJ0QbiMmEtCiR3AHlDiapaQ9m'),
  1,
  @PCGroupId
);
SET @AdminUserId = SCOPE_IDENTITY();

-- 4) Assign roles to admin (Admin and Support)
DECLARE
  @AdminRoleId   INT,
  @SupportRoleId INT;

SELECT @AdminRoleId   = RoleId FROM auth.AppRoles WHERE RoleName = 'Admin';
SELECT @SupportRoleId = RoleId FROM auth.AppRoles WHERE RoleName = 'Support';

INSERT INTO auth.UserRoles (UserId, RoleId) VALUES (@AdminUserId, @AdminRoleId);
INSERT INTO auth.UserRoles (UserId, RoleId) VALUES (@AdminUserId, @SupportRoleId);
GO

-- Verify inserts
SELECT * FROM auth.Users;
SELECT * FROM auth.AppRoles;
SELECT * FROM auth.UserRoles;
SELECT * FROM crm.Companies;


UPDATE auth.Users SET Email = 'diviv78453@jazipo.com', PasswordHash = CONVERT(VARBINARY(256), '$2a$12$t5dMcFCPdvt.EBXvdb1Kee8tNnVUmJ0QbiMmEtCiR3AHlDiapaQ9m')
WHERE UserId=1

DELETE FROM auth.UsersHistory WHERE UserId = 2

DELETE FROM auth.UserRoles WHERE UserId = 2


DELETE FROM auth.Users WHERE UserId = 2


DELETE FROM auth.AuthTokens WHERE UserId = 2