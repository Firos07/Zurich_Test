-- =====================================================================
-- 05_seed_data.sql
-- Gestor de Siniestros Simplificado
-- Datos iniciales de prueba (sin informacion personal real).
-- Incluye siniestros en los tres estados y con historial de estados.
-- Ejecutar con: sqlcmd -S localhost\SQLEXPRESS -E -d ClaimsDb -i 05_seed_data.sql
-- =====================================================================

IF EXISTS (SELECT 1 FROM [dbo].[Claims])
BEGIN
    PRINT N'Claims ya contiene datos. Seed omitido.';
    RETURN;
END

DECLARE @c1 INT, @c2 INT, @c3 INT, @c4 INT, @c5 INT, @c6 INT, @c7 INT, @c8 INT, @c9 INT, @c10 INT;

-- ---------------------------------------------------------------
-- 3 siniestros OPEN (historial: OPEN inicial)
-- ---------------------------------------------------------------
INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1001', N'María González', N'Auto',      8500.00, N'OPEN',      DATEADD(DAY, -1,  GETUTCDATE()), DATEADD(DAY, -1,  GETUTCDATE()));
SET @c1 = SCOPE_IDENTITY();

INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1002', N'Carlos Pérez',   N'Hogar',    15000.00, N'OPEN',      DATEADD(DAY, -2,  GETUTCDATE()), DATEADD(DAY, -2,  GETUTCDATE()));
SET @c2 = SCOPE_IDENTITY();

INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1003', N'Lucía Fernández', N'Salud',    3200.00,  N'OPEN',      DATEADD(HOUR, -8, GETUTCDATE()), DATEADD(HOUR, -8, GETUTCDATE()));
SET @c3 = SCOPE_IDENTITY();

INSERT INTO [dbo].[ClaimStatusHistory] (ClaimId, PreviousStatus, NewStatus, ChangedAt)
VALUES (@c1, NULL, N'OPEN', DATEADD(DAY, -1,  GETUTCDATE())),
       (@c2, NULL, N'OPEN', DATEADD(DAY, -2,  GETUTCDATE())),
       (@c3, NULL, N'OPEN', DATEADD(HOUR, -8, GETUTCDATE()));

-- ---------------------------------------------------------------
-- 4 siniestros IN_REVIEW (historial: OPEN -> IN_REVIEW)
-- ---------------------------------------------------------------
INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1004', N'Jorge Rodríguez', N'Auto',  24000.00, N'IN_REVIEW', DATEADD(DAY, -6,  GETUTCDATE()), DATEADD(DAY, -1,  GETUTCDATE()));
SET @c4 = SCOPE_IDENTITY();

INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1005', N'Ana Martínez',    N'Hogar', 9800.00,  N'IN_REVIEW', DATEADD(DAY, -4,  GETUTCDATE()), DATEADD(DAY, -2,  GETUTCDATE()));
SET @c5 = SCOPE_IDENTITY();

INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1006', N'Pedro Sánchez',   N'Viaje',  750.00,   N'IN_REVIEW', DATEADD(DAY, -5,  GETUTCDATE()), DATEADD(DAY, -1,  GETUTCDATE()));
SET @c6 = SCOPE_IDENTITY();

INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1005', N'Ana Martínez',    N'Auto',  5200.00,  N'IN_REVIEW', DATEADD(DAY, -3,  GETUTCDATE()), DATEADD(DAY, -2,  GETUTCDATE()));
SET @c7 = SCOPE_IDENTITY();

INSERT INTO [dbo].[ClaimStatusHistory] (ClaimId, PreviousStatus, NewStatus, ChangedAt)
VALUES (@c4, NULL,          N'OPEN',      DATEADD(DAY, -6, GETUTCDATE())),
       (@c4, N'OPEN',       N'IN_REVIEW', DATEADD(DAY, -1, GETUTCDATE())),
       (@c5, NULL,          N'OPEN',      DATEADD(DAY, -4, GETUTCDATE())),
       (@c5, N'OPEN',       N'IN_REVIEW', DATEADD(DAY, -2, GETUTCDATE())),
       (@c6, NULL,          N'OPEN',      DATEADD(DAY, -5, GETUTCDATE())),
       (@c6, N'OPEN',       N'IN_REVIEW', DATEADD(DAY, -1, GETUTCDATE())),
       (@c7, NULL,          N'OPEN',      DATEADD(DAY, -3, GETUTCDATE())),
       (@c7, N'OPEN',       N'IN_REVIEW', DATEADD(DAY, -2, GETUTCDATE()));

-- ---------------------------------------------------------------
-- 3 siniestros CLOSED (historial: OPEN -> IN_REVIEW -> CLOSED)
-- ---------------------------------------------------------------
INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1007', N'Sofía Díaz',     N'Vida',   20000.00, N'CLOSED', DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()));
SET @c8 = SCOPE_IDENTITY();

INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1008', N'Diego Torres',   N'Hogar',   6350.00,  N'CLOSED', DATEADD(DAY, -20, GETUTCDATE()), DATEADD(DAY, -5,  GETUTCDATE()));
SET @c9 = SCOPE_IDENTITY();

INSERT INTO [dbo].[Claims] (PolicyNumber, InsuredName, ClaimType, EstimatedAmount, Status, CreatedAt, UpdatedAt)
VALUES (N'POL-1009', N'Elena Ramírez',  N'Auto',    11000.00, N'CLOSED', DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, -3,  GETUTCDATE()));
SET @c10 = SCOPE_IDENTITY();

INSERT INTO [dbo].[ClaimStatusHistory] (ClaimId, PreviousStatus, NewStatus, ChangedAt)
VALUES (@c8,  NULL,          N'OPEN',      DATEADD(DAY, -30, GETUTCDATE())),
       (@c8,  N'OPEN',       N'IN_REVIEW', DATEADD(DAY, -25, GETUTCDATE())),
       (@c8,  N'IN_REVIEW',  N'CLOSED',    DATEADD(DAY, -10, GETUTCDATE())),
       (@c9,  NULL,          N'OPEN',      DATEADD(DAY, -20, GETUTCDATE())),
       (@c9,  N'OPEN',       N'IN_REVIEW', DATEADD(DAY, -12, GETUTCDATE())),
       (@c9,  N'IN_REVIEW',  N'CLOSED',    DATEADD(DAY, -5,  GETUTCDATE())),
       (@c10, NULL,          N'OPEN',      DATEADD(DAY, -15, GETUTCDATE())),
       (@c10, N'OPEN',       N'IN_REVIEW', DATEADD(DAY, -8,  GETUTCDATE())),
       (@c10, N'IN_REVIEW',  N'CLOSED',    DATEADD(DAY, -3,  GETUTCDATE()));

PRINT N'Seed data inserted: 10 claims with status history.';