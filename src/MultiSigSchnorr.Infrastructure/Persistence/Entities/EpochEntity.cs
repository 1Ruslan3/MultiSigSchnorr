namespace MultiSigSchnorr.Infrastructure.Persistence.Entities;

public sealed class EpochEntity
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
    public DateTime? ActivatedUtc { get; set; }
    public DateTime? ClosedUtc { get; set; }
}