namespace MultiSigSchnorr.Domain.Enums;

public enum AuditEventType
{
    ParticipantRegistered = 0,
    ParticipantRevoked = 1,
    EpochCreated = 2,
    EpochActivated = 3,
    SessionCreated = 4,
    CommitmentSubmitted = 5,
    NonceRevealed = 6,
    PartialSignatureSubmitted = 7,
    AggregateSignatureCreated = 8,
    SignatureVerified = 9,
    Error = 10
}