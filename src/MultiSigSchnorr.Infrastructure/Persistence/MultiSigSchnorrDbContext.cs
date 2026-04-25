using Microsoft.EntityFrameworkCore;
using MultiSigSchnorr.Infrastructure.Persistence.Entities;

namespace MultiSigSchnorr.Infrastructure.Persistence;

public sealed class MultiSigSchnorrDbContext : DbContext
{
    public MultiSigSchnorrDbContext(DbContextOptions<MultiSigSchnorrDbContext> options)
        : base(options)
    {
    }

    public DbSet<EpochEntity> Epochs => Set<EpochEntity>();
    public DbSet<ParticipantEntity> Participants => Set<ParticipantEntity>(); 
    public DbSet<EpochMemberEntity> EpochMembers => Set<EpochMemberEntity>();
    public DbSet<AuditLogEntryEntity> AuditLogEntries => Set<AuditLogEntryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EpochEntity>(entity =>
        {
            entity.ToTable("epochs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Number)
                .HasColumnName("number")
                .IsRequired();

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.CreatedUtc)
                .HasColumnName("created_utc")
                .IsRequired();

            entity.Property(x => x.ActivatedUtc)
                .HasColumnName("activated_utc");

            entity.Property(x => x.ClosedUtc)
                .HasColumnName("closed_utc");

            entity.HasIndex(x => x.Number)
                .IsUnique();
        });

        modelBuilder.Entity<ParticipantEntity>(entity =>
        {
            entity.ToTable("participants");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(x => x.PublicKeyHex)
                .HasColumnName("public_key_hex")
                .IsRequired();

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.CreatedUtc)
                .HasColumnName("created_utc")
                .IsRequired();

            entity.Property(x => x.RevokedUtc)
                .HasColumnName("revoked_utc");

            entity.HasIndex(x => x.DisplayName);
        });

        modelBuilder.Entity<EpochMemberEntity>(entity =>
        {
            entity.ToTable("epoch_members");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.EpochId)
                .HasColumnName("epoch_id")
                .IsRequired();

            entity.Property(x => x.ParticipantId)
                .HasColumnName("participant_id")
                .IsRequired();

            entity.Property(x => x.AddedUtc)
                .HasColumnName("added_utc")
                .IsRequired();

            entity.Property(x => x.RemovedUtc)
                .HasColumnName("removed_utc");

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.HasIndex(x => new { x.EpochId, x.ParticipantId })
                .IsUnique();

            entity.HasOne<EpochEntity>()
                .WithMany()
                .HasForeignKey(x => x.EpochId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ParticipantEntity>()
                .WithMany()
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLogEntryEntity>(entity =>
        {
            entity.ToTable("audit_log_entries");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.ActionType)
                .HasColumnName("action_type")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.EntityType)
                .HasColumnName("entity_type")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.EntityId)
                .HasColumnName("entity_id");

            entity.Property(x => x.Description)
                .HasColumnName("description")
                .IsRequired();

            entity.Property(x => x.MetadataJson)
                .HasColumnName("metadata_json")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(x => x.CreatedUtc)
                .HasColumnName("created_utc")
                .IsRequired();

            entity.HasIndex(x => x.CreatedUtc);
            entity.HasIndex(x => x.ActionType);
            entity.HasIndex(x => x.EntityType);
            entity.HasIndex(x => x.EntityId);
        });
    }
}