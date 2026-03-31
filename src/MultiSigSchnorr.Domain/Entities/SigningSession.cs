using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class SigningSession
{
    public Guid Id { get; private set; }
    public Guid EpochId { get; private set; }
    public string Message { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime? CompletedUtc { get; private set; }

    public SigningSession(Guid id, Guid epochId, string message, DateTime createdUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(id));
        if (epochId == Guid.Empty)
            throw new ArgumentException("Epoch id cannot be empty.", nameof(epochId));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        Id = id;
        EpochId = epochId;
        Message = message;
        CreatedUtc = createdUtc;
        Status = SessionStatus.Created;
    }

    public void StartCommitmentsCollection() => Status = SessionStatus.CommitmentsCollection;
    public void StartNonceRevealCollection() => Status = SessionStatus.NonceRevealCollection;
    public void StartPartialSignaturesCollection() => Status = SessionStatus.PartialSignaturesCollection;

    public void Complete(DateTime completedUtc)
    {
        Status = SessionStatus.Completed;
        CompletedUtc = completedUtc;
    }

    public void Fail() => Status = SessionStatus.Failed;
    public void Cancel() => Status = SessionStatus.Cancelled;

    private SigningSession() { }
}