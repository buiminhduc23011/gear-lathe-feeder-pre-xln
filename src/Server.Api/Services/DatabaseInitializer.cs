using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server.Api.Data;
using Server.Api.Data.Entities;
using Server.Api.Infrastructure;
using Server.Api.Options;

namespace Server.Api.Services;

public sealed class DatabaseInitializer
{
    private const string SqlServerSchemaBootstrap = """
        IF OBJECT_ID(N'[dbo].[Machines]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[Machines]
            (
                [MachineId] INT IDENTITY(1,1) NOT NULL,
                [MachineCode] NVARCHAR(50) NOT NULL,
                [MachineName] NVARCHAR(150) NOT NULL,
                [Manufacturer] NVARCHAR(100) NOT NULL,
                [Description] NVARCHAR(500) NOT NULL,
                [Model] NVARCHAR(100) NULL,
                [SerialNumber] NVARCHAR(100) NULL,
                [Location] NVARCHAR(200) NULL,
                [Jig1HeightMm] REAL NOT NULL,
                [Jig2HeightMm] REAL NOT NULL,
                [Jig3HeightMm] REAL NOT NULL,
                [Jig4HeightMm] REAL NOT NULL,
                [AssignedStagingSlot1] INT NULL,
                [AssignedStagingSlot2] INT NULL,
                [IsActive] BIT NOT NULL,
                [CreatedAtUtc] DATETIMEOFFSET NOT NULL,
                [UpdatedAtUtc] DATETIMEOFFSET NOT NULL,
                CONSTRAINT [PK_Machines] PRIMARY KEY ([MachineId])
            );
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'Jig1HeightMm') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] ADD [Jig1HeightMm] REAL NOT NULL CONSTRAINT [DF_Machines_Jig1HeightMm] DEFAULT 0;
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'Jig2HeightMm') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] ADD [Jig2HeightMm] REAL NOT NULL CONSTRAINT [DF_Machines_Jig2HeightMm] DEFAULT 0;
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'Jig3HeightMm') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] ADD [Jig3HeightMm] REAL NOT NULL CONSTRAINT [DF_Machines_Jig3HeightMm] DEFAULT 0;
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'Jig4HeightMm') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] ADD [Jig4HeightMm] REAL NOT NULL CONSTRAINT [DF_Machines_Jig4HeightMm] DEFAULT 0;
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'AssignedStagingSlot1') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] ADD [AssignedStagingSlot1] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'AssignedStagingSlot2') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] ADD [AssignedStagingSlot2] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'PlcHost') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [PlcHost];
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'PlcPort') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [PlcPort];
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'PlcSlaveId') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [PlcSlaveId];
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'PlcConnectionMode') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [PlcConnectionMode];
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'PollIntervalMs') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [PollIntervalMs];
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'ApiBaseUrl') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [ApiBaseUrl];
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'AutoReconnect') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [AutoReconnect];
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'ReconnectIntervalMs') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [ReconnectIntervalMs];
        END;

        IF COL_LENGTH(N'[dbo].[Machines]', N'MaxRetry') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[Machines] DROP COLUMN [MaxRetry];
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_Machines_MachineCode'
              AND object_id = OBJECT_ID(N'[dbo].[Machines]', N'U'))
        BEGIN
            CREATE UNIQUE INDEX [IX_Machines_MachineCode]
                ON [dbo].[Machines] ([MachineCode]);
        END;

        IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[Users]
            (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [Username] NVARCHAR(100) NOT NULL,
                [PasswordHash] NVARCHAR(1024) NOT NULL,
                [FullName] NVARCHAR(150) NOT NULL,
                [Email] NVARCHAR(255) NULL,
                [Role] NVARCHAR(50) NOT NULL,
                [IsActive] BIT NOT NULL,
                [IsSystemAccount] BIT NOT NULL,
                [CreatedAtUtc] DATETIMEOFFSET NOT NULL,
                [UpdatedAtUtc] DATETIMEOFFSET NOT NULL,
                CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_Users_Username'
              AND object_id = OBJECT_ID(N'[dbo].[Users]', N'U'))
        BEGIN
            CREATE UNIQUE INDEX [IX_Users_Username]
                ON [dbo].[Users] ([Username]);
        END;

        IF OBJECT_ID(N'[dbo].[UploadedFiles]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[UploadedFiles]
            (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [OriginalFileName] NVARCHAR(260) NOT NULL,
                [StoredFileName] NVARCHAR(260) NOT NULL,
                [StoragePath] NVARCHAR(500) NOT NULL,
                [Size] BIGINT NOT NULL,
                [MachineName] NVARCHAR(150) NOT NULL,
                [Manufacturer] NVARCHAR(100) NOT NULL,
                [SentAtUtc] DATETIMEOFFSET NOT NULL,
                [UploadedAtUtc] DATETIMEOFFSET NOT NULL,
                [UploadSource] NVARCHAR(30) NOT NULL,
                [UploadedByUserId] INT NULL,
                [IsDeleted] BIT NOT NULL,
                [DeletedAtUtc] DATETIMEOFFSET NULL,
                [DeletedByUserId] INT NULL,
                CONSTRAINT [PK_UploadedFiles] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_UploadedFiles_UploadedAtUtc'
              AND object_id = OBJECT_ID(N'[dbo].[UploadedFiles]', N'U'))
        BEGIN
            CREATE INDEX [IX_UploadedFiles_UploadedAtUtc]
                ON [dbo].[UploadedFiles] ([UploadedAtUtc] DESC);
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_UploadedFiles_IsDeleted_UploadedAtUtc'
              AND object_id = OBJECT_ID(N'[dbo].[UploadedFiles]', N'U'))
        BEGIN
            CREATE INDEX [IX_UploadedFiles_IsDeleted_UploadedAtUtc]
                ON [dbo].[UploadedFiles] ([IsDeleted], [UploadedAtUtc] DESC);
        END;

        IF OBJECT_ID(N'[dbo].[UploadedFileActions]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[UploadedFileActions]
            (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [UploadedFileId] INT NOT NULL,
                [ActionType] NVARCHAR(50) NOT NULL,
                [PerformedByUserId] INT NOT NULL,
                [PerformedByUsernameSnapshot] NVARCHAR(100) NOT NULL,
                [PerformedAtUtc] DATETIMEOFFSET NOT NULL,
                CONSTRAINT [PK_UploadedFileActions] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_UploadedFileActions_UploadedFileId'
              AND object_id = OBJECT_ID(N'[dbo].[UploadedFileActions]', N'U'))
        BEGIN
            CREATE INDEX [IX_UploadedFileActions_UploadedFileId]
                ON [dbo].[UploadedFileActions] ([UploadedFileId]);
        END;

        IF OBJECT_ID(N'[dbo].[ModelProfiles]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[ModelProfiles]
            (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [MachineId] INT NOT NULL,
                [ModelName] NVARCHAR(150) NOT NULL,
                [ItemType] NVARCHAR(150) NULL,
                [MachiningProgram] INT NULL,
                [Spare1] NVARCHAR(150) NULL,
                [Spare2] NVARCHAR(150) NULL,
                [OuterShaftDiameter] DECIMAL(18,3) NULL,
                [DiameterOp1] REAL NULL,
                [DiameterOp2] REAL NULL,
                [InputBlankDiameter] REAL NULL,
                [Op2ChuckSleeveDepth] REAL NULL,
                [TrayUsage] INT NULL,
                [TrayType] INT NULL,
                [OrderInput] INT NULL,
                [RobotData] NVARCHAR(MAX) NOT NULL,
                [Line1Data] NVARCHAR(MAX) NOT NULL,
                [Line2Data] NVARCHAR(MAX) NOT NULL,
                [CreatedByUsername] NVARCHAR(100) NULL,
                [UpdatedByUsername] NVARCHAR(100) NULL,
                [CreatedAtUtc] DATETIMEOFFSET NOT NULL,
                [UpdatedAtUtc] DATETIMEOFFSET NOT NULL,
                [IsDeleted] BIT NOT NULL DEFAULT 0,
                [DeletedAtUtc] DATETIMEOFFSET NULL,
                [DeletedByUsername] NVARCHAR(100) NULL,
                CONSTRAINT [PK_ModelProfiles] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_ModelProfiles_Machines_MachineId]
                    FOREIGN KEY ([MachineId])
                    REFERENCES [dbo].[Machines] ([MachineId])
                    ON DELETE CASCADE
            );
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'IsDeleted') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles]
                ADD [IsDeleted] BIT NOT NULL DEFAULT 0,
                    [DeletedAtUtc] DATETIMEOFFSET NULL,
                    [DeletedByUsername] NVARCHAR(100) NULL;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ModelProfiles_MachineId_ModelName'
              AND object_id = OBJECT_ID(N'[dbo].[ModelProfiles]', N'U'))
        BEGIN
            CREATE UNIQUE INDEX [IX_ModelProfiles_MachineId_ModelName]
                ON [dbo].[ModelProfiles] ([MachineId], [ModelName]);
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ModelProfiles_IsDeleted'
              AND object_id = OBJECT_ID(N'[dbo].[ModelProfiles]', N'U'))
        BEGIN
            CREATE INDEX [IX_ModelProfiles_IsDeleted]
                ON [dbo].[ModelProfiles] ([IsDeleted]);
        END;

        IF OBJECT_ID(N'[dbo].[ModelProfileSnapshots]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[ModelProfileSnapshots]
            (
                [Id] BIGINT IDENTITY(1,1) NOT NULL,
                [ModelProfileId] INT NOT NULL,
                [ModelName] NVARCHAR(150) NOT NULL,
                [ItemType] NVARCHAR(150) NULL,
                [MachiningProgram] INT NULL,
                [Spare1] NVARCHAR(150) NULL,
                [Spare2] NVARCHAR(150) NULL,
                [OuterShaftDiameter] DECIMAL(18,3) NULL,
                [DiameterOp1] REAL NULL,
                [DiameterOp2] REAL NULL,
                [InputBlankDiameter] REAL NULL,
                [Op2ChuckSleeveDepth] REAL NULL,
                [TrayUsage] INT NULL,
                [TrayType] INT NULL,
                [OrderInput] INT NULL,
                [RobotData] NVARCHAR(MAX) NOT NULL,
                [Line1Data] NVARCHAR(MAX) NOT NULL,
                [Line2Data] NVARCHAR(MAX) NOT NULL,
                [ChangeAction] NVARCHAR(20) NOT NULL,
                [PerformedByUsername] NVARCHAR(100) NOT NULL,
                [PerformedAtUtc] DATETIMEOFFSET NOT NULL,
                CONSTRAINT [PK_ModelProfileSnapshots] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_ModelProfileSnapshots_ModelProfiles_ModelProfileId]
                    FOREIGN KEY ([ModelProfileId])
                    REFERENCES [dbo].[ModelProfiles] ([Id])
                    ON DELETE CASCADE
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ModelProfileSnapshots_ModelProfileId'
              AND object_id = OBJECT_ID(N'[dbo].[ModelProfileSnapshots]', N'U'))
        BEGIN
            CREATE INDEX [IX_ModelProfileSnapshots_ModelProfileId]
                ON [dbo].[ModelProfileSnapshots] ([ModelProfileId]);
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'IsEnabled') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles]
                ADD [IsEnabled] BIT NOT NULL DEFAULT 0;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'IsEnabled') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots]
                ADD [IsEnabled] BIT NOT NULL DEFAULT 0;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'ItemType') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [ItemType] NVARCHAR(150) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'MachiningProgram') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [MachiningProgram] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'Spare1') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [Spare1] NVARCHAR(150) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'Spare2') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [Spare2] NVARCHAR(150) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'OuterShaftDiameter') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [OuterShaftDiameter] DECIMAL(18,3) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'DiameterOp1') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [DiameterOp1] REAL NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'DiameterOp2') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [DiameterOp2] REAL NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'InputBlankDiameter') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [InputBlankDiameter] REAL NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'Op2ChuckSleeveDepth') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [Op2ChuckSleeveDepth] REAL NULL;
        END;

        IF EXISTS (
            SELECT 1
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID(N'[dbo].[ModelProfiles]', N'U')
              AND c.name = N'DiameterOp1'
              AND t.name IN (N'decimal', N'numeric'))
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ALTER COLUMN [DiameterOp1] REAL NULL;
        END;

        IF EXISTS (
            SELECT 1
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID(N'[dbo].[ModelProfiles]', N'U')
              AND c.name = N'DiameterOp2'
              AND t.name IN (N'decimal', N'numeric'))
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ALTER COLUMN [DiameterOp2] REAL NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'TrayUsage') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [TrayUsage] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'TrayType') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [TrayType] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfiles]', N'OrderInput') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfiles] ADD [OrderInput] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'ItemType') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [ItemType] NVARCHAR(150) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'MachiningProgram') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [MachiningProgram] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'Spare1') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [Spare1] NVARCHAR(150) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'Spare2') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [Spare2] NVARCHAR(150) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'OuterShaftDiameter') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [OuterShaftDiameter] DECIMAL(18,3) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'DiameterOp1') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [DiameterOp1] REAL NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'DiameterOp2') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [DiameterOp2] REAL NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'InputBlankDiameter') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [InputBlankDiameter] REAL NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'Op2ChuckSleeveDepth') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [Op2ChuckSleeveDepth] REAL NULL;
        END;

        IF EXISTS (
            SELECT 1
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID(N'[dbo].[ModelProfileSnapshots]', N'U')
              AND c.name = N'DiameterOp1'
              AND t.name IN (N'decimal', N'numeric'))
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ALTER COLUMN [DiameterOp1] REAL NULL;
        END;

        IF EXISTS (
            SELECT 1
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID(N'[dbo].[ModelProfileSnapshots]', N'U')
              AND c.name = N'DiameterOp2'
              AND t.name IN (N'decimal', N'numeric'))
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ALTER COLUMN [DiameterOp2] REAL NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'TrayUsage') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [TrayUsage] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'TrayType') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [TrayType] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ModelProfileSnapshots]', N'OrderInput') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ModelProfileSnapshots] ADD [OrderInput] INT NULL;
        END;

        IF OBJECT_ID(N'[dbo].[ManualShelfDeclarations]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[ManualShelfDeclarations]
            (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [MachineId] INT NOT NULL,
                [Mode] NVARCHAR(20) NOT NULL DEFAULT 'Agv',
                [StagingSlotIndex] INT NULL,
                [MachineSlotIndex] INT NULL,
                [ShelfLayoutType] INT NOT NULL,
                [OrdersJson] NVARCHAR(MAX) NOT NULL,
                [Status] NVARCHAR(30) NOT NULL,
                [MachineCodeSnapshot] NVARCHAR(50) NOT NULL DEFAULT '',
                [MachineNameSnapshot] NVARCHAR(150) NOT NULL DEFAULT '',
                [CreatedByUsername] NVARCHAR(100) NOT NULL,
                [CreatedAtUtc] DATETIMEOFFSET NOT NULL,
                [PickedByAgvId] NVARCHAR(100) NULL,
                [PickedByAgvName] NVARCHAR(150) NULL,
                [AgvTakenAtUtc] DATETIMEOFFSET NULL,
                [LoadRequestedAtUtc] DATETIMEOFFSET NULL,
                [LoadedAtUtc] DATETIMEOFFSET NULL,
                [ProductionStartedAtUtc] DATETIMEOFFSET NULL,
                [CompletedAtUtc] DATETIMEOFFSET NULL,
                [ClearedAtUtc] DATETIMEOFFSET NULL,
                [CancelledAtUtc] DATETIMEOFFSET NULL,
                [ProductionDurationSeconds] INT NULL,
                [UpdatedAtUtc] DATETIMEOFFSET NOT NULL,
                CONSTRAINT [PK_ManualShelfDeclarations] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_ManualShelfDeclarations_Machines_MachineId]
                    FOREIGN KEY ([MachineId])
                    REFERENCES [dbo].[Machines] ([MachineId])
                    ON DELETE CASCADE
            );
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'Mode') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations]
                ADD [Mode] NVARCHAR(20) NOT NULL CONSTRAINT [DF_ManualShelfDeclarations_Mode] DEFAULT 'ManualLoad';
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'StagingSlotIndex') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [StagingSlotIndex] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'MachineSlotIndex') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [MachineSlotIndex] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'MachineCodeSnapshot') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [MachineCodeSnapshot] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ManualShelfDeclarations_MachineCodeSnapshot] DEFAULT '';
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'MachineNameSnapshot') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [MachineNameSnapshot] NVARCHAR(150) NOT NULL CONSTRAINT [DF_ManualShelfDeclarations_MachineNameSnapshot] DEFAULT '';
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'PickedByAgvId') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [PickedByAgvId] NVARCHAR(100) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'PickedByAgvName') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [PickedByAgvName] NVARCHAR(150) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'AgvTakenAtUtc') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [AgvTakenAtUtc] DATETIMEOFFSET NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'LoadRequestedAtUtc') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [LoadRequestedAtUtc] DATETIMEOFFSET NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'LoadedAtUtc') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [LoadedAtUtc] DATETIMEOFFSET NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'ProductionStartedAtUtc') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [ProductionStartedAtUtc] DATETIMEOFFSET NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'CompletedAtUtc') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [CompletedAtUtc] DATETIMEOFFSET NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'ClearedAtUtc') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [ClearedAtUtc] DATETIMEOFFSET NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'CancelledAtUtc') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [CancelledAtUtc] DATETIMEOFFSET NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'CancelledByUsername') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [CancelledByUsername] NVARCHAR(100) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'ClearedByUsername') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [ClearedByUsername] NVARCHAR(100) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'ProductionDurationSeconds') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [ProductionDurationSeconds] INT NULL;
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'UpdatedAtUtc') IS NULL
        BEGIN
            ALTER TABLE [dbo].[ManualShelfDeclarations] ADD [UpdatedAtUtc] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_ManualShelfDeclarations_UpdatedAtUtc] DEFAULT SYSDATETIMEOFFSET();
        END;

        IF COL_LENGTH(N'[dbo].[ManualShelfDeclarations]', N'AppliedAtUtc') IS NOT NULL
        BEGIN
            EXEC(N'
                UPDATE d
                SET [Status] = CASE
                        WHEN d.[Status] = ''Pending'' THEN ''Created''
                        WHEN d.[Status] = ''Applied'' THEN ''InProduction''
                        WHEN d.[Status] = ''Cancelled'' THEN ''Cancelled''
                        ELSE d.[Status]
                    END,
                    [Mode] = COALESCE(NULLIF(d.[Mode], ''''), ''ManualLoad''),
                    [MachineSlotIndex] = COALESCE(d.[MachineSlotIndex], d.[KeIndex]),
                    [MachineCodeSnapshot] = CASE WHEN d.[MachineCodeSnapshot] = '''' THEN COALESCE(m.[MachineCode], '''') ELSE d.[MachineCodeSnapshot] END,
                    [MachineNameSnapshot] = CASE WHEN d.[MachineNameSnapshot] = '''' THEN COALESCE(m.[MachineName], '''') ELSE d.[MachineNameSnapshot] END,
                    [ProductionStartedAtUtc] = COALESCE(d.[ProductionStartedAtUtc], d.[AppliedAtUtc]),
                    [UpdatedAtUtc] = COALESCE(d.[UpdatedAtUtc], d.[CreatedAtUtc])
                FROM [dbo].[ManualShelfDeclarations] d
                INNER JOIN [dbo].[Machines] m ON m.[MachineId] = d.[MachineId];
            ');
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ManualShelfDeclarations_MachineId_Mode_Status'
              AND object_id = OBJECT_ID(N'[dbo].[ManualShelfDeclarations]', N'U'))
        BEGIN
            EXEC(N'
                CREATE INDEX [IX_ManualShelfDeclarations_MachineId_Mode_Status]
                    ON [dbo].[ManualShelfDeclarations] ([MachineId], [Mode], [Status]);
            ');
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ManualShelfDeclarations_MachineId_StagingSlotIndex'
              AND object_id = OBJECT_ID(N'[dbo].[ManualShelfDeclarations]', N'U'))
        BEGIN
            EXEC(N'
                CREATE INDEX [IX_ManualShelfDeclarations_MachineId_StagingSlotIndex]
                    ON [dbo].[ManualShelfDeclarations] ([MachineId], [StagingSlotIndex]);
            ');
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ManualShelfDeclarations_MachineId_MachineSlotIndex'
              AND object_id = OBJECT_ID(N'[dbo].[ManualShelfDeclarations]', N'U'))
        BEGIN
            EXEC(N'
                CREATE INDEX [IX_ManualShelfDeclarations_MachineId_MachineSlotIndex]
                    ON [dbo].[ManualShelfDeclarations] ([MachineId], [MachineSlotIndex]);
            ');
        END;

        IF OBJECT_ID(N'[dbo].[ShelfDeclarationEvents]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[ShelfDeclarationEvents]
            (
                [Id] BIGINT IDENTITY(1,1) NOT NULL,
                [DeclarationId] INT NOT NULL,
                [EventType] NVARCHAR(30) NOT NULL,
                [EventAtUtc] DATETIMEOFFSET NOT NULL,
                [ActorType] NVARCHAR(20) NOT NULL,
                [ActorId] NVARCHAR(100) NULL,
                [ActorName] NVARCHAR(150) NULL,
                [MachineId] INT NULL,
                [MachineSlotIndex] INT NULL,
                [StagingSlotIndex] INT NULL,
                [PayloadJson] NVARCHAR(MAX) NULL,
                CONSTRAINT [PK_ShelfDeclarationEvents] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_ShelfDeclarationEvents_ManualShelfDeclarations_DeclarationId]
                    FOREIGN KEY ([DeclarationId])
                    REFERENCES [dbo].[ManualShelfDeclarations] ([Id])
                    ON DELETE CASCADE
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ShelfDeclarationEvents_DeclarationId_EventAtUtc'
              AND object_id = OBJECT_ID(N'[dbo].[ShelfDeclarationEvents]', N'U'))
        BEGIN
            CREATE INDEX [IX_ShelfDeclarationEvents_DeclarationId_EventAtUtc]
                ON [dbo].[ShelfDeclarationEvents] ([DeclarationId], [EventAtUtc]);
        END;
        """;

    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<UserEntity> _passwordHasher;
    private readonly SeedDataOptions _seedOptions;

    public DatabaseInitializer(
        AppDbContext dbContext,
        IPasswordHasher<UserEntity> passwordHasher,
        IOptions<SeedDataOptions> seedOptions)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _seedOptions = seedOptions.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (string.Equals(_dbContext.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

            if (string.Equals(_dbContext.Database.ProviderName, "Microsoft.EntityFrameworkCore.SqlServer", StringComparison.Ordinal))
            {
                await _dbContext.Database.ExecuteSqlRawAsync(SqlServerSchemaBootstrap, cancellationToken);
            }
        }

        await SeedUsersAsync(cancellationToken);
        await SeedMachinesAsync(cancellationToken);
        await BackfillMachineStagingSlotsAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var admin = new UserEntity
        {
            Username = _seedOptions.AdminUsername.Trim(),
            FullName = _seedOptions.AdminFullName.Trim(),
            Role = AppRoles.Admin,
            IsActive = true,
            IsSystemAccount = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        admin.PasswordHash = _passwordHasher.HashPassword(admin, _seedOptions.AdminPassword);

        var technician = new UserEntity
        {
            Username = _seedOptions.TechnicianUsername.Trim(),
            FullName = _seedOptions.TechnicianFullName.Trim(),
            Role = AppRoles.Technician,
            IsActive = true,
            IsSystemAccount = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        technician.PasswordHash = _passwordHasher.HashPassword(technician, _seedOptions.TechnicianPassword);

        _dbContext.Users.AddRange(admin, technician);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedMachinesAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Machines.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.Machines.Add(new MachineEntity
        {
            MachineCode = "GLF-01",
            MachineName = "Gear Lathe Feeder Pre-XLN 01",
            Manufacturer = "STI",
            Description = "",
            AssignedStagingSlot1 = 1,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task BackfillMachineStagingSlotsAsync(CancellationToken cancellationToken)
    {
        var machines = await _dbContext.Machines
            .OrderBy(x => x.MachineId)
            .ToListAsync(cancellationToken);

        if (machines.Count == 0)
        {
            return;
        }

        var usedSlots = new HashSet<int>(
            machines
                .Select(GetExistingStagingSlot)
                .Where(slot => slot.HasValue)
                .Select(slot => slot!.Value));

        var changed = false;
        foreach (var machine in machines)
        {
            var assignedSlot = GetExistingStagingSlot(machine);
            if (!assignedSlot.HasValue)
            {
                var availableSlot = Enumerable.Range(1, 4)
                    .FirstOrDefault(slot => !usedSlots.Contains(slot));
                if (availableSlot > 0)
                {
                    assignedSlot = availableSlot;
                    usedSlots.Add(availableSlot);
                }
            }

            if (machine.AssignedStagingSlot1 != assignedSlot || machine.AssignedStagingSlot2 is not null)
            {
                machine.AssignedStagingSlot1 = assignedSlot;
                machine.AssignedStagingSlot2 = null;
                machine.UpdatedAtUtc = DateTimeOffset.UtcNow;
                changed = true;
            }
        }

        if (changed)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static int? GetExistingStagingSlot(MachineEntity machine)
    {
        return machine.AssignedStagingSlot1 is >= 1 and <= 4
            ? machine.AssignedStagingSlot1
            : null;
    }
}
