namespace MultiSigSchnorr.Infrastructure.Persistence.Entities;

public sealed class EpochMemberEntity
{
    public Guid Id { get; set; }
    public Guid EpochId { get; set; }
    public Guid ParticipantId { get; set; }

    public DateTime AddedUtc { get; set; }
    public DateTime? RemovedUtc { get; set; }

    public bool IsActive { get; set; }
}