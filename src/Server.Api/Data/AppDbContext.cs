using Microsoft.EntityFrameworkCore;
using Server.Api.Data.Entities;

namespace Server.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<MachineEntity> Machines => Set<MachineEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<UploadedFileEntity> UploadedFiles => Set<UploadedFileEntity>();

    public DbSet<UploadedFileActionEntity> UploadedFileActions => Set<UploadedFileActionEntity>();

    public DbSet<ModelProfileEntity> ModelProfiles => Set<ModelProfileEntity>();

    public DbSet<ModelProfileSnapshotEntity> ModelProfileSnapshots => Set<ModelProfileSnapshotEntity>();

    public DbSet<ManualShelfDeclarationEntity> ManualShelfDeclarations => Set<ManualShelfDeclarationEntity>();

    public DbSet<ShelfDeclarationEventEntity> ShelfDeclarationEvents => Set<ShelfDeclarationEventEntity>();
    public DbSet<ReportEntity> Reports => Set<ReportEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MachineEntity>(entity =>
        {
            entity.ToTable("Machines");
            entity.HasKey(x => x.MachineId);
            entity.HasIndex(x => x.MachineCode).IsUnique();

            entity.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.MachineName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Manufacturer).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.Model).HasMaxLength(100);
            entity.Property(x => x.SerialNumber).HasMaxLength(100);
            entity.Property(x => x.Location).HasMaxLength(200);
            entity.Property(x => x.Jig1HeightMm).HasColumnType("real");
            entity.Property(x => x.Jig2HeightMm).HasColumnType("real");
            entity.Property(x => x.Jig3HeightMm).HasColumnType("real");
            entity.Property(x => x.Jig4HeightMm).HasColumnType("real");
            entity.Property(x => x.AssignedStagingSlot1);
            entity.Property(x => x.AssignedStagingSlot2);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Username).IsUnique();

            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.Role).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<UploadedFileEntity>(entity =>
        {
            entity.ToTable("UploadedFiles");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.MachineName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Manufacturer).HasMaxLength(100).IsRequired();
            entity.Property(x => x.UploadSource).HasMaxLength(30).IsRequired();
            entity.Property(x => x.SentAtUtc).IsRequired();
            entity.Property(x => x.UploadedAtUtc).IsRequired();
            entity.HasIndex(x => x.UploadedAtUtc);
            entity.HasIndex(x => x.IsDeleted);
        });

        modelBuilder.Entity<UploadedFileActionEntity>(entity =>
        {
            entity.ToTable("UploadedFileActions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PerformedByUsernameSnapshot).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PerformedAtUtc).IsRequired();
            entity.HasIndex(x => x.UploadedFileId);
        });

        modelBuilder.Entity<ModelProfileEntity>(entity =>
        {
            entity.ToTable("ModelProfiles");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.MachineId, x.ModelName }).IsUnique();

            entity.Property(x => x.ModelName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ItemType).HasMaxLength(150);
            entity.Property(x => x.Spare1).HasMaxLength(150);
            entity.Property(x => x.Spare2).HasMaxLength(150);
            entity.Property(x => x.OuterShaftDiameter).HasPrecision(18, 3);
            entity.Property(x => x.InputBlankDiameter).HasColumnType("real");
            entity.Property(x => x.Op2ChuckSleeveDepth).HasColumnType("real");
            entity.Property(x => x.RobotData).IsRequired();
            entity.Property(x => x.Line1Data).IsRequired();
            entity.Property(x => x.Line2Data).IsRequired();
            entity.Property(x => x.CreatedByUsername).HasMaxLength(100);
            entity.Property(x => x.UpdatedByUsername).HasMaxLength(100);
            entity.Property(x => x.DeletedByUsername).HasMaxLength(100);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.Property(x => x.IsEnabled).IsRequired();
            entity.HasIndex(x => x.IsDeleted);

            entity.HasOne(x => x.Machine)
                .WithMany()
                .HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ModelProfileSnapshotEntity>(entity =>
        {
            entity.ToTable("ModelProfileSnapshots");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ModelProfileId);

            entity.Property(x => x.ModelName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ItemType).HasMaxLength(150);
            entity.Property(x => x.Spare1).HasMaxLength(150);
            entity.Property(x => x.Spare2).HasMaxLength(150);
            entity.Property(x => x.OuterShaftDiameter).HasPrecision(18, 3);
            entity.Property(x => x.InputBlankDiameter).HasColumnType("real");
            entity.Property(x => x.Op2ChuckSleeveDepth).HasColumnType("real");
            entity.Property(x => x.RobotData).IsRequired();
            entity.Property(x => x.Line1Data).IsRequired();
            entity.Property(x => x.Line2Data).IsRequired();
            entity.Property(x => x.ChangeAction).HasMaxLength(20).IsRequired();
            entity.Property(x => x.IsEnabled).IsRequired();
            entity.Property(x => x.PerformedByUsername).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PerformedAtUtc).IsRequired();

            entity.HasOne(x => x.ModelProfile)
                .WithMany(x => x.Snapshots)
                .HasForeignKey(x => x.ModelProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ManualShelfDeclarationEntity>(entity =>
        {
            entity.ToTable("ManualShelfDeclarations");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.MachineId, x.Mode, x.Status });
            entity.HasIndex(x => new { x.MachineId, x.StagingSlotIndex });
            entity.HasIndex(x => new { x.MachineId, x.MachineSlotIndex });

            entity.Property(x => x.Mode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.OrdersJson).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.MachineCodeSnapshot).HasMaxLength(50).IsRequired();
            entity.Property(x => x.MachineNameSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CreatedByUsername).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PickedByAgvId).HasMaxLength(100);
            entity.Property(x => x.PickedByAgvName).HasMaxLength(150);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();

            entity.HasOne(x => x.Machine)
                .WithMany()
                .HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShelfDeclarationEventEntity>(entity =>
        {
            entity.ToTable("ShelfDeclarationEvents");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.DeclarationId, x.EventAtUtc });

            entity.Property(x => x.EventType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ActorType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ActorId).HasMaxLength(100);
            entity.Property(x => x.ActorName).HasMaxLength(150);
            entity.Property(x => x.PayloadJson);

            entity.HasOne(x => x.Declaration)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.DeclarationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReportEntity>(entity =>
        {
            entity.ToTable("Reports");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.MachineCode, x.ReportDate }).IsUnique();

            entity.Property(x => x.MachineCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReportDate).IsRequired();
            entity.Property(x => x.TotalJobs).IsRequired();
            entity.Property(x => x.PassedJobs).IsRequired();
            entity.Property(x => x.FailedJobs).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });
    }
}
