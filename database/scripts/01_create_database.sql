-- =====================================================================
-- 01_create_database.sql
-- Gestor de Siniestros Simplificado
-- Crea la base de datos ClaimsDb.
-- Ejecutar con: sqlcmd -S localhost\SQLEXPRESS -E -i 01_create_database.sql
-- =====================================================================

IF DB_ID(N'ClaimsDb') IS NULL
BEGIN
    CREATE DATABASE [ClaimsDb];
    PRINT N'Database [ClaimsDb] created.';
END
ELSE
BEGIN
    PRINT N'Database [ClaimsDb] already exists.';
END
GO