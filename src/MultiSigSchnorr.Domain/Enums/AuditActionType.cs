namespace MultiSigSchnorr.Domain.Enums;

public enum AuditActionType
{
    ProtocolSessionCreated = 0,
    ParticipantRevoked = 1,
    EpochTransitioned = 2,
    ParticipantCreated = 3,
    ParticipantRenamed = 4,
    EpochCreatedWithMembers = 5
}
