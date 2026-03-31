namespace MultiSigSchnorr.Domain.Entities;

public sealed class SessionMember
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public DateTime JoinedUtc { get; private set; }

    public SessionMember(Guid id, Guid sessionId, Guid participantId, DateTime joinedUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Session member id cannot be empty.", nameof(id));
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));

        Id = id;
        SessionId = sessionId;
        ParticipantId = participantId;
        JoinedUtc = joinedUtc;
    }

    private SessionMember() { }
}