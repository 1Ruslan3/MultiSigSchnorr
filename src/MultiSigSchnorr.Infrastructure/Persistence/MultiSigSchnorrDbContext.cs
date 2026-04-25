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

    public DbSet<ProtocolSessionProjectionEntity> ProtocolSessions => Set<ProtocolSessionProjectionEntity>();
    public DbSet<ProtocolSessionParticipantProjectionEntity> ProtocolSessionParticipants => Set<ProtocolSessionParticipantProjectionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EpochEntity>(entity =>
        {
            entity.ToTable("epochs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");

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

            entity.HasIndex(x => x.Number).IsUnique();
        });

        modelBuilder.Entity<ParticipantEntity>(entity =>
        {
            entity.ToTable("participants");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");

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

            entity.Property(x => x.Id).HasColumnName("id");

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

            entity.Property(x => x.Id).HasColumnName("id");

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

        modelBuilder.Entity<ProtocolSessionProjectionEntity>(entity =>
        {
            entity.ToTable("protocol_sessions");

            entity.HasKey(x => x.SessionId);

            entity.Property(x => x.SessionId)
                .HasColumnName("session_id");

            entity.Property(x => x.EpochId)
                .HasColumnName("epoch_id")
                .IsRequired();

            entity.Property(x => x.EpochNumber)
                .HasColumnName("epoch_number")
                .IsRequired();

            entity.Property(x => x.SessionStatus)
                .HasColumnName("session_status")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.ProtectionMode)
                .HasColumnName("protection_mode")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.CreatedUtc)
                .HasColumnName("created_utc")
                .IsRequired();

            entity.Property(x => x.CompletedUtc)
                .HasColumnName("completed_utc");

            entity.Property(x => x.MessageDigestHex)
                .HasColumnName("message_digest_hex")
                .IsRequired();

            entity.Property(x => x.AggregatePublicKeyHex)
                .HasColumnName("aggregate_public_key_hex")
                .IsRequired();

            entity.Property(x => x.AggregateNoncePointHex)
                .HasColumnName("aggregate_nonce_point_hex");

            entity.Property(x => x.ChallengeHex)
                .HasColumnName("challenge_hex");

            entity.Property(x => x.AggregateSignatureNoncePointHex)
                .HasColumnName("aggregate_signature_nonce_point_hex");

            entity.Property(x => x.AggregateSignatureScalarHex)
                .HasColumnName("aggregate_signature_scalar_hex");

            entity.Property(x => x.AllCommitmentsPublished)
                .HasColumnName("all_commitments_published")
                .IsRequired();

            entity.Property(x => x.AllNoncesRevealed)
                .HasColumnName("all_nonces_revealed")
                .IsRequired();

            entity.Property(x => x.AllPartialSignaturesSubmitted)
                .HasColumnName("all_partial_signatures_submitted")
                .IsRequired();

            entity.HasIndex(x => x.CreatedUtc);
            entity.HasIndex(x => x.EpochId);
            entity.HasIndex(x => x.SessionStatus);
            entity.HasIndex(x => x.ProtectionMode);

            entity.HasMany(x => x.Participants)
                .WithOne(x => x.Session)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProtocolSessionParticipantProjectionEntity>(entity =>
        {
            entity.ToTable("protocol_session_participants");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.SessionId)
                .HasColumnName("session_id")
                .IsRequired();

            entity.Property(x => x.ParticipantId)
                .HasColumnName("participant_id")
                .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(x => x.HasCommitment)
                .HasColumnName("has_commitment")
                .IsRequired();

            entity.Property(x => x.HasReveal)
                .HasColumnName("has_reveal")
                .IsRequired();

            entity.Property(x => x.HasPartialSignature)
                .HasColumnName("has_partial_signature")
                .IsRequired();

            entity.Property(x => x.PublicKeyHex)
                .HasColumnName("public_key_hex")
                .IsRequired();

            entity.Property(x => x.AggregationCoefficientHex)
                .HasColumnName("aggregation_coefficient_hex")
                .IsRequired();

            entity.Property(x => x.CommitmentHex)
                .HasColumnName("commitment_hex");

            entity.Property(x => x.PublicNoncePointHex)
                .HasColumnName("public_nonce_point_hex");

            entity.Property(x => x.PartialSignatureHex)
                .HasColumnName("partial_signature_hex");

            entity.HasIndex(x => new { x.SessionId, x.ParticipantId })
                .IsUnique();
        });
    }
}