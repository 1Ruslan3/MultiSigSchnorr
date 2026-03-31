using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class Epoch
{
    public Guid Id { get; private set; }
    public int Number { get; private set; }
    public EpochStatus Status { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime? ActivatedUtc { get; private set; }
    public DateTime? ClosedUtc { get; private set; }

    public Epoch(Guid id, int number, DateTime createdUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Epoch id cannot be empty.", nameof(id));
        if (number < 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        Id = id;
        Number = number;
        Status = EpochStatus.Draft;
        CreatedUtc = createdUtc;
    }

    public void Activate(DateTime activatedUtc)
    {
        if (Status is EpochStatus.Closed or EpochStatus.Archived)
            throw new InvalidOperationException("Closed or archived epoch cannot be activated.");

        Status = EpochStatus.Active;
        ActivatedUtc = activatedUtc;
    }

    public void Close(DateTime closedUtc)
    {
        if (Status == EpochStatus.Archived)
            throw new InvalidOperationException("Archived epoch cannot be closed.");

        Status = EpochStatus.Closed;
        ClosedUtc = closedUtc;
    }

    private Epoch() { }
}