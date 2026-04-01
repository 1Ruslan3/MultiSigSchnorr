using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Protocol.Epochs;

public sealed class EpochTransitionResult
{
    private readonly IReadOnlyList<EpochMember> _newEpochMembers;

    public Epoch PreviousEpoch { get; }
    public Epoch NewEpoch { get; }
    public IReadOnlyList<EpochMember> NewEpochMembers => _newEpochMembers;

    public EpochTransitionResult(
        Epoch previousEpoch,
        Epoch newEpoch,
        IReadOnlyList<EpochMember> newEpochMembers)
    {
        PreviousEpoch = previousEpoch ?? throw new ArgumentNullException(nameof(previousEpoch));
        NewEpoch = newEpoch ?? throw new ArgumentNullException(nameof(newEpoch));
        _newEpochMembers = newEpochMembers ?? throw new ArgumentNullException(nameof(newEpochMembers));
    }
}