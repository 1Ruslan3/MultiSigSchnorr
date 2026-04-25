namespace MultiSigSchnorr.Infrastructure.Persistence.Entities;

public sealed class ParticipantEntity
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string PublicKeyHex { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
}