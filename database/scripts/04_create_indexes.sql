-- =====================================================================
-- 04_create_indexes.sql
-- Gestor de Siniestros Simplificado
-- Indices justificados segun las consultas principales de la aplicacion.
-- Ejecutar con: sqlcmd -S localhost\SQLEXPRESS -E -d ClaimsDb -i 04_create_indexes.sql
--
-- Justificacion de cada indice:
--   * IX_Claims_Status
--       La pantalla principal filtra por Status (GET /api/claims?status=...).
--       Un indice no agrupado sobre Status acelera el filtrado.
--   * IX_Claims_PolicyNumber
--       La aplicacion permite buscar por numero de poliza
--       (GET /api/claims?policyNumber=...). No es un indice unico porque
--       una misma poliza puede tener varios siniestros a lo largo del tiempo.
--   * IX_ClaimStatusHistory_ClaimId
--       Consultar el historial de un siniestro (GET /api/claims/{id}/history)
--       accede siempre por ClaimId. El indice tambien respalda la FK.
-- =====================================================================

CREATE INDEX [IX_Claims_Status] ON [dbo].[Claims] ([Status] ASC);
GO

CREATE INDEX [IX_Claims_PolicyNumber] ON [dbo].[Claims] ([PolicyNumber] ASC);
GO

CREATE INDEX [IX_ClaimStatusHistory_ClaimId] ON [dbo].[ClaimStatusHistory] ([ClaimId] ASC);
GO