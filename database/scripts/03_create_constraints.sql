-- =====================================================================
-- 03_create_constraints.sql
-- Gestor de Siniestros Simplificado
-- Restricciones de integridad: valores por defecto, CHECK y FOREIGN KEY.
-- Ejecutar con: sqlcmd -S localhost\SQLEXPRESS -E -d ClaimsDb -i 03_create_constraints.sql
-- =====================================================================

-- Valores por defecto para fechas (se almacenan en UTC de forma consistente)
ALTER TABLE [dbo].[Claims] ADD CONSTRAINT [DF_Claims_CreatedAt]
    DEFAULT (SYSUTCDATETIME()) FOR [CreatedAt];
GO

ALTER TABLE [dbo].[Claims] ADD CONSTRAINT [DF_Claims_UpdatedAt]
    DEFAULT (SYSUTCDATETIME()) FOR [UpdatedAt];
GO

-- Todo siniestro nuevo comienza en OPEN
ALTER TABLE [dbo].[Claims] ADD CONSTRAINT [DF_Claims_Status]
    DEFAULT (N'OPEN') FOR [Status];
GO

-- El monto estimado debe ser mayor que cero
ALTER TABLE [dbo].[Claims] ADD CONSTRAINT [CK_Claims_EstimatedAmount]
    CHECK ([EstimatedAmount] > 0);
GO

-- El estado solo puede ser OPEN, IN_REVIEW o CLOSED
ALTER TABLE [dbo].[Claims] ADD CONSTRAINT [CK_Claims_Status]
    CHECK ([Status] IN (N'OPEN', N'IN_REVIEW', N'CLOSED'));
GO

ALTER TABLE [dbo].[ClaimStatusHistory] ADD CONSTRAINT [DF_ClaimStatusHistory_ChangedAt]
    DEFAULT (SYSUTCDATETIME()) FOR [ChangedAt];
GO

ALTER TABLE [dbo].[ClaimStatusHistory] ADD CONSTRAINT [CK_ClaimStatusHistory_NewStatus]
    CHECK ([NewStatus] IN (N'OPEN', N'IN_REVIEW', N'CLOSED'));
GO

-- PreviousStatus admite NULL (registro inicial del siniestro) o un estado valido
ALTER TABLE [dbo].[ClaimStatusHistory] ADD CONSTRAINT [CK_ClaimStatusHistory_PreviousStatus]
    CHECK ([PreviousStatus] IS NULL OR [PreviousStatus] IN (N'OPEN', N'IN_REVIEW', N'CLOSED'));
GO

-- Relacion Claims 1 ---- N ClaimStatusHistory
-- ON DELETE CASCADE: el historial pertenece al siniestro, se elimina con el.
ALTER TABLE [dbo].[ClaimStatusHistory] ADD CONSTRAINT [FK_ClaimStatusHistory_Claim]
    FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[Claims] ([Id]) ON DELETE CASCADE;
GO