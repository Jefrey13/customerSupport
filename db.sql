
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




----------------------------------------------------------------------------------
USE CustomerSupportDB;

SELECT * FROM auth.Menus;
truncate table auth.menus
GO
BEGIN
	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Dashboard', 'Analisi de los KPI de las interacciónes con los clientes', 'Dashboard', 1, 'LayoutDashboard');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Chat', 'Pagina de mensajes entre PC GROUP S.A. y clientes', 'Chat', 2, 'MessageSquare');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Usuarios', 'Listados de usuarios en el sistema', 'Usuarios', 3, 'Users');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Notificaciones', 'Asignación de conversación, nuevo usuario, etc.', 'Notificaciones', 4, 'BellRing');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Perfil', 'Perfil a detalle del usuario', 'Perfil', 5, 'User');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Cerrar Sesión', 'Redirije por defecto al inicio de sesión', 'Cerrar-sesión', 6, 'LogOut');
END


  SELEct * FROM auth.AppRoles
  SELEct * FROM auth.Menus
  select * from auth.RoleMenus;

  truncate table RoleMenus
GO
BEGIN
	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 1),
		(1, 1),
		(2, 1),
		(3, 1),
		(4, 1),
		(5, 1),
		(6, 1)


	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 3),
		(2, 3),
		(4, 3),
		(5, 3),
		(6, 3);


	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 2),
		(2, 2),
		(4, 2),
		(5, 2),
		(6, 2);
END


USE CustomerSupportDB;

SELECT * FROM auth.AppRoles;
SELECT * FROM auth.Users;
SELECT * FROM auth.UserRoles;
SELECT * FROM auth.AuthTokens;;
SELECT * FROM chat.Conversations
SELECT * FROM chat.Messages

DELETe chat.Messages;
DELETE chat.Conversations;
DELETE auth.AuthTokens;
DELETE auth.UserRoles;
DELETE auth.Users;





GO
BEGIN
	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Dashboard', 'Analisi de los KPI de las interacciónes con los clientes', 'Dashboard', 1, 'LayoutDashboard');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Chat', 'Pagina de mensajes entre PC GROUP S.A. y clientes', 'Chat', 2, 'MessageSquare');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Usuarios', 'Listados de usuarios en el sistema', 'Usuarios', 3, 'Users');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Notificaciones', 'Asignación de conversación, nuevo usuario, etc.', 'Notificaciones', 4, 'BellRing');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Perfil', 'Perfil a detalle del usuario', 'Perfil', 5, 'User');

	INSERT INTO [auth].[Menus]([Name], [Description], [Url], [Index], Icon) 
	VALUES ('Cerrar Sesión', 'Redirije por defecto al inicio de sesión', 'Cerrar-sesión', 6, 'LogOut');
END

GO
BEGIN
	INSERT INTO 
		[auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 1),
		(2, 1),
		(3, 1),
		(4, 1),
		(5, 1),
		(6, 1);


	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 3),
		(2, 3),
		(4, 3),
		(5, 3),
		(6, 3);


	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 2),
		(2, 2),
		(4, 2),
		(5, 2),
		(6, 2);
END




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
   WHERE u.Email = 'wocelo3429@neuraxo.com'
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
    'Administrator',
    'wocelo3429@neuraxo.com',
    CONVERT(VARBINARY(256), '$2a$12$rrrJjwcb4qviYmCdGD8XTOt8vulpJxXf5BozSJ2xVLdgTPfRFp90a'),
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
   WHERE Email = 'wocelo3429@neuraxo.com';
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



USE CustomerSupportDB;

SELECT * FROM auth.AppRoles;
SELECT * FROM auth.Users;
SELECT * FROM auth.UserRoles;
SELECT * FROM auth.AuthTokens;;
SELECT * FROM chat.Conversations
SELECT * FROM chat.Messages
SELECT * FROM auth.Menus
SELECT * FROM auth.RoleMenus

SELECT * FROM chat.Messages
DELETe chat.Messages;
DELETE chat.Conversations;
DELETE auth.AuthTokens;
DELETE auth.UserRoles;
DELETE auth.Users;
DELETE auth.RoleMenus;
--Truncate table auth.Menus;

---------------------------------------------------------
  SELEct * FROM auth.AppRoles
  SELEct * FROM auth.Menus
  select * from auth.RoleMenus;

  truncate table RoleMenus
GO
BEGIN
	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 1),
		(1, 1),
		(2, 1),
		(3, 1),
		(4, 1),
		(5, 1),
		(6, 1)


	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 3),
		(2, 3),
		(4, 3),
		(5, 3),
		(6, 3);


	INSERT INTO [auth].[RoleMenus]([MenuId], [RoleId]) 
	VALUES 
		(1, 2),
		(2, 2),
		(4, 2),
		(5, 2),
		(6, 2);
END