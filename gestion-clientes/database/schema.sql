IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Clientes] (
    [Id] int NOT NULL IDENTITY,
    [Nombres] nvarchar(100) NOT NULL,
    [Apellidos] nvarchar(100) NOT NULL,
    [Ruc] nchar(13) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Telefono] nvarchar(20) NOT NULL,
    [Direccion] nvarchar(250) NOT NULL,
    [FechaRegistro] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Clientes_Ruc] ON [Clientes] ([Ruc]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260829182236_InitialCreate', N'10.0.11');

COMMIT;
GO
