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
    [CompanyId] INT IDENTITY(1,1)       NOT NULL,
    [Name]      NVARCHAR(150)           NOT NULL,
    [Address]   NVARCHAR(200)           NULL,        
    [CreatedAt] DATETIME2(7)            NOT NULL
        DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED([CompanyId])
) ON [PRIMARY];
GO

/****** Tables in auth schema ******/
-- 1) UsersHistory (history table)
CREATE TABLE [auth].[UsersHistory](
    [UserId]          INT            NOT NULL,
    [FullName]        NVARCHAR(100)  NULL,
    [Email]           NVARCHAR(255)  MASKED WITH (FUNCTION = 'email()') NULL,
    [PasswordHash]    VARBINARY(256) NULL,
    [IsActive]        BIT            NOT NULL,
    [SecurityStamp]   UNIQUEIDENTIFIER NOT NULL,
    [ConcurrencyStamp] UNIQUEIDENTIFIER NOT NULL,
    [CompanyId]       INT            NULL,
    [Phone]           NVARCHAR(20)   NULL,
    [Identifier]      NVARCHAR(50)   NULL,
    [CreatedAt]       DATETIME2(7)   NOT NULL,
    [UpdatedAt]       DATETIME2(7)   NULL,
    [ImageUrl]        NVARCHAR(500)  NULL,           
    [RowVersion]      TIMESTAMP      NOT NULL,
    [DataRequested]   BIT            NOT NULL DEFAULT(0),
    [ValidFrom]       DATETIME2(7)   NOT NULL,
    [ValidTo]         DATETIME2(7)   NOT NULL
) ON [PRIMARY];
GO
CREATE CLUSTERED INDEX [ix_UsersHistory]
    ON [auth].[UsersHistory]([ValidTo],[ValidFrom]);
GO

-- 2) Users (with system-versioning)
CREATE TABLE [auth].[Users](
    [UserId]          INT            IDENTITY(1,1) NOT NULL,
    [FullName]        NVARCHAR(100)  NULL,
    [Email]           NVARCHAR(255)  MASKED WITH (FUNCTION = 'email()') NULL,
    [PasswordHash]    VARBINARY(256) NULL,
    [IsActive]        BIT            NOT NULL DEFAULT (0),
    [SecurityStamp]   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [ConcurrencyStamp] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [CompanyId]       INT            NULL,
    [Phone]           NVARCHAR(20)   NULL,
    [DataRequested]   BIT            NOT NULL CONSTRAINT DF_Users_DataRequested DEFAULT(0),
    [Identifier]      NVARCHAR(50)   NULL,
    [CreatedAt]       DATETIME2(7)   NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]       DATETIME2(7)   NULL,
    [ImageUrl]        NVARCHAR(500)  NULL,         
    [RowVersion]      TIMESTAMP      NOT NULL,
    [ValidFrom]       DATETIME2(7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo]         DATETIME2(7) GENERATED ALWAYS AS ROW END   HIDDEN NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED([UserId]),
    PERIOD FOR SYSTEM_TIME ([ValidFrom],[ValidTo])
)  
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [auth].[UsersHistory]));
GO

ALTER TABLE [auth].[Users]
    ADD CONSTRAINT FK_Users_Companies
        FOREIGN KEY([CompanyId]) REFERENCES [crm].[Companies]([CompanyId]);
GO

-- 3) AppRoles
CREATE TABLE [auth].[AppRoles](
    [RoleId]      INT            IDENTITY(1,1) NOT NULL,
    [RoleName]    NVARCHAR(50)   NOT NULL,
    [Description] NVARCHAR(200)  NULL,
    [CreatedAt]   DATETIME2(7)   NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]   DATETIME2(7)   NULL,
    [RowVersion]  TIMESTAMP      NOT NULL,
    CONSTRAINT [PK_AppRoles] PRIMARY KEY CLUSTERED([RoleId])
) ON [PRIMARY];
GO
ALTER TABLE [auth].[AppRoles]
    ADD CONSTRAINT UQ_AppRoles_RoleName UNIQUE([RoleName]);
GO

-- 4) AuthTokens
CREATE TABLE [auth].[AuthTokens](
    [TokenId]   INT            IDENTITY(1,1) NOT NULL,
    [UserId]    INT            NOT NULL,
    [TokenType] NVARCHAR(50)   NOT NULL,
    [Token]     NVARCHAR(500)  NOT NULL,
    [CreatedAt] DATETIME2(7)   NOT NULL DEFAULT SYSUTCDATETIME(),
    [ExpiresAt] DATETIME2(7)   NOT NULL,
    [Revoked]   BIT            NOT NULL DEFAULT (0),
    [Used]      BIT            NOT NULL DEFAULT (0),
    [RowVersion] TIMESTAMP     NOT NULL,
    CONSTRAINT [PK_AuthTokens] PRIMARY KEY CLUSTERED([TokenId])
) ON [PRIMARY];
GO
ALTER TABLE [auth].[AuthTokens]
    ADD CONSTRAINT FK_AuthTokens_Users FOREIGN KEY([UserId]) REFERENCES [auth].[Users]([UserId]);
GO

-- 5) UserRoles
CREATE TABLE [auth].[UserRoles](
    [UserId]     INT          NOT NULL,
    [RoleId]     INT          NOT NULL,
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
    [ConversationId] INT            IDENTITY(1,1) PRIMARY KEY,
    [CompanyId]      INT            NULL,
    [ClientUserId]   INT            NULL,
    [AssignedAgent]  INT            NULL,
    [AssignedBy]     INT            NULL,
    [AssignedAt]     DATETIME2(7)   NULL,
    [Status]         NVARCHAR(20)   NOT NULL DEFAULT 'Bot',
    [CreatedAt]      DATETIME2(7)   NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]      DATETIME2(7)   NULL,
    CONSTRAINT FK_Conversations_Companies FOREIGN KEY([CompanyId])      REFERENCES [crm].[Companies]([CompanyId]),
    CONSTRAINT FK_Conversations_Client    FOREIGN KEY([ClientUserId])   REFERENCES [auth].[Users]([UserId]),
    CONSTRAINT FK_Conversations_Agent     FOREIGN KEY([AssignedAgent])  REFERENCES [auth].[Users]([UserId]),
    CONSTRAINT FK_Conversations_AssignedBy FOREIGN KEY([AssignedBy])     REFERENCES [auth].[Users]([UserId])
) ON [PRIMARY];
GO

-- Messages
CREATE TABLE [chat].[Messages](
    [MessageId]      INT            IDENTITY(1,1) PRIMARY KEY,
    [ConversationId] INT            NOT NULL,
    [SenderId]       INT            NOT NULL,
    [Content]        NVARCHAR(MAX)  NULL,
    [ExternalId]     NVARCHAR(50)   NULL,
    [MessageType]    NVARCHAR(20)   NOT NULL,
    [CreatedAt]      DATETIME2(7)   NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Messages_Conversations FOREIGN KEY([ConversationId]) REFERENCES [chat].[Conversations]([ConversationId]),
    CONSTRAINT FK_Messages_Sender        FOREIGN KEY([SenderId])       REFERENCES [auth].[Users]([UserId])
) ON [PRIMARY];
GO

-- Attachments
CREATE TABLE [chat].[Attachments](
    [AttachmentId] INT            IDENTITY(1,1) PRIMARY KEY,
    [MessageId]    INT            NOT NULL,
    [MediaId]      NVARCHAR(100)  NOT NULL,
    [FileName]     NVARCHAR(200)  NULL,
    [MediaUrl]     NVARCHAR(500)  NULL,
    [MimeType]     NVARCHAR(100)  NULL,
    [CreatedAt]    DATETIME2(7)   NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Attachments_Messages FOREIGN KEY([MessageId]) REFERENCES [chat].[Messages]([MessageId])
) ON [PRIMARY];
GO

USE [CustomerSupportDB];
GO

-- =================================================================================
-- Seed data: Companies, Roles and Admin User
-- =================================================================================

-- 1) Insert 6 companies (including PC Group S.A) + dirección
INSERT INTO crm.Companies (Name, Address) VALUES
  ('PC Group S.A',             'Av. Central 123, Ciudad'),
  ('Distribuidora La Esquina', 'Calle Primera 45, Colonia'),
  ('Panadería El Trigal',      'Calle Pan 10, Barrio'),
  ('Farmacia Vida Sana',       'Av. Salud 78, Zona'),
  ('Hotel Paraíso Colonial',   'Carrera 5 #34-56, Centro'),
  ('Librería El Saber',        'Calle Libro 200, Distrito');
GO

-- 2) Insert AppRoles (if not exists)
INSERT INTO auth.AppRoles (RoleName, Description)
SELECT v.RoleName, v.Description
FROM (VALUES
   ('Admin',    'System administrator with full access'),
   ('Customer', 'Client using the platform for service management'),
   ('Support',  'Customer support personnel in charge of communication')
) AS v(RoleName, Description)
WHERE NOT EXISTS (
  SELECT 1 FROM auth.AppRoles r
   WHERE r.RoleName = v.RoleName
);
GO

-- 3+4) Insert admin user for PC Group S.A (si no existe) + asignar roles
DECLARE
  @AdminUserId   INT,
  @PCGroupId     INT,
  @AdminRoleId   INT,
  @SupportRoleId INT;

/* Obtener CompanyId */
SELECT @PCGroupId = CompanyId
  FROM crm.Companies
 WHERE Name = 'PC Group S.A';

/* Insertar usuario administrador si no existe */
IF NOT EXISTS (
  SELECT 1 FROM auth.Users u
   WHERE u.Email = 'diviv78453@jazipo.com'
)
BEGIN
  INSERT INTO auth.Users (
      FullName,
      Email,
      PasswordHash,
      IsActive,
      CompanyId,
      DataRequested,    -- obligatorio
      ImageUrl
  )
  VALUES (
    'John Administrator',
    'diviv78453@jazipo.com',
    CONVERT(VARBINARY(256), '$2a$12$CHPmeJUDpRUtsPDjyKO/3uVM2WhYUilAMYyXovQ1oACgdc.c.LNf.'),
    1,
    @PCGroupId,
    0,  -- DataRequested = false
    'https://i.ibb.co/CpHzbxg9/logopcg.webp'
  );

  SET @AdminUserId = SCOPE_IDENTITY();
END
ELSE
BEGIN
  SELECT @AdminUserId = UserId
    FROM auth.Users
   WHERE Email = 'diviv78453@jazipo.com';
END

/* Obtener RoleId de Admin y Support */
SELECT @AdminRoleId   = RoleId FROM auth.AppRoles WHERE RoleName = 'Admin';
SELECT @SupportRoleId = RoleId FROM auth.AppRoles WHERE RoleName = 'Support';

/* Asignar rol Admin */
IF NOT EXISTS (
  SELECT 1 FROM auth.UserRoles ur
   WHERE ur.UserId = @AdminUserId
     AND ur.RoleId = @AdminRoleId
)
  INSERT INTO auth.UserRoles (UserId, RoleId)
  VALUES (@AdminUserId, @AdminRoleId);

/* Asignar rol Support */
IF NOT EXISTS (
  SELECT 1 FROM auth.UserRoles ur
   WHERE ur.UserId = @AdminUserId
     AND ur.RoleId = @SupportRoleId
)
  INSERT INTO auth.UserRoles (UserId, RoleId)
  VALUES (@AdminUserId, @SupportRoleId);
GO





USE [CustomerSupportDB];
GO

-- =================================================================================
-- Seed data: Companies, Roles and Admin User
-- =================================================================================

-- 1) Insert 6 companies (including PC Group S.A) + dirección
INSERT INTO crm.Companies (Name, Address) VALUES
  ('PC Group S.A',             'Av. Central 123, Ciudad'),
  ('Distribuidora La Esquina', 'Calle Primera 45, Colonia'),
  ('Panadería El Trigal',      'Calle Pan 10, Barrio'),
  ('Farmacia Vida Sana',       'Av. Salud 78, Zona'),
  ('Hotel Paraíso Colonial',   'Carrera 5 #34-56, Centro'),
  ('Librería El Saber',        'Calle Libro 200, Distrito');
GO

-- 2) Insert AppRoles (if not exists)
INSERT INTO auth.AppRoles (RoleName, Description)
SELECT v.RoleName, v.Description
FROM (VALUES
   ('Admin',    'System administrator with full access'),
   ('Customer', 'Client using the platform for service management'),
   ('Support',  'Customer support personnel in charge of communication')
) AS v(RoleName, Description)
WHERE NOT EXISTS (
  SELECT 1 FROM auth.AppRoles r
   WHERE r.RoleName = v.RoleName
);
GO

-- 3+4) Insert admin user for PC Group S.A (si no existe) + asignar roles
DECLARE
  @AdminUserId   INT,
  @PCGroupId     INT,
  @AdminRoleId   INT,
  @SupportRoleId INT;

/* Obtener CompanyId */
SELECT @PCGroupId = CompanyId
  FROM crm.Companies
 WHERE Name = 'PC Group S.A';

/* Insertar usuario administrador si no existe */
IF NOT EXISTS (
  SELECT 1 FROM auth.Users u
   WHERE u.Email = 'diviv78453@jazipo.com'
)
BEGIN
  INSERT INTO auth.Users (
      FullName,
      Email,
      PasswordHash,
      IsActive,
      CompanyId,
      DataRequested,    -- obligatorio
      ImageUrl
  )
  VALUES (
    'John Administrator',
    'diviv78453@jazipo.com',
    CONVERT(VARBINARY(256), '$2a$12$CHPmeJUDpRUtsPDjyKO/3uVM2WhYUilAMYyXovQ1oACgdc.c.LNf.'),
    1,
    @PCGroupId,
    0,  -- DataRequested = false
    'https://i.ibb.co/CpHzbxg9/logopcg.webp'
  );

  SET @AdminUserId = SCOPE_IDENTITY();
END
ELSE
BEGIN
  SELECT @AdminUserId = UserId
    FROM auth.Users
   WHERE Email = 'diviv78453@jazipo.com';
END

/* Obtener RoleId de Admin y Support */
SELECT @AdminRoleId   = RoleId FROM auth.AppRoles WHERE RoleName = 'Admin';
SELECT @SupportRoleId = RoleId FROM auth.AppRoles WHERE RoleName = 'Support';

/* Asignar rol Admin */
IF NOT EXISTS (
  SELECT 1 FROM auth.UserRoles ur
   WHERE ur.UserId = @AdminUserId
     AND ur.RoleId = @AdminRoleId
)
  INSERT INTO auth.UserRoles (UserId, RoleId)
  VALUES (@AdminUserId, @AdminRoleId);

/* Asignar rol Support */
IF NOT EXISTS (
  SELECT 1 FROM auth.UserRoles ur
   WHERE ur.UserId = @AdminUserId
     AND ur.RoleId = @SupportRoleId
)
  INSERT INTO auth.UserRoles (UserId, RoleId)
  VALUES (@AdminUserId, @SupportRoleId);
GO
