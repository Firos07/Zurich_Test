-- =====================================================================
-- 02_create_tables.sql
-- Gestor de Siniestros Simplificado
-- Crea las tablas Claims y ClaimStatusHistory.
-- Ejecutar con: sqlcmd -S localhost\SQLEXPRESS -E -d ClaimsDb -i 02_create_tables.sql
-- =====================================================================

CREATE TABLE [dbo].[ClaimStatusHistory]
(
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [ClaimId]        INT            NOT NULL,
    [PreviousStatus] NVARCHAR (20)  NULL,
    [NewStatus]      NVARCHAR (20)  NOT NULL,
    [ChangedAt]      DATETIME2      NOT NULL,
    CONSTRAINT [PK_ClaimStatusHistory] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Claims]
(
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [PolicyNumber]    NVARCHAR (50)  NOT NULL,
    [InsuredName]     NVARCHAR (200) NOT NULL,
    [ClaimType]       NVARCHAR (100) NOT NULL,
    [EstimatedAmount] DECIMAL (18, 2) NOT NULL,
    [Status]          NVARCHAR (20)  NOT NULL,
    [CreatedAt]       DATETIME2      NOT NULL,
    [UpdatedAt]       DATETIME2      NOT NULL,
    CONSTRAINT [PK_Claims] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO