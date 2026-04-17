using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Protocol.Models;

public sealed class NPartyProtocolSession
{
    private readonly IReadOnlyDictionary<Guid, NPartyParticipantProtocolState> _participants;
    private readonly IReadOnlyList<SessionMember> _sessionMembers;

    public Epoch Epoch { get; }
    public SigningSession SigningSession { get; }
    public MessageDigestValue MessageDigest { get; }
    public PublicKeyValue AggregatePublicKey { get; }
    public SignatureProtectionMode ProtectionMode { get; }

    public PointValue? AggregateNoncePoint { get; private set; }
    public ScalarValue? Challenge { get; private set; }
    public AggregateSignature? AggregateSignature { get; private set; }

    public IReadOnlyDictionary<Guid, NPartyParticipantProtocolState> Participants => _participants;
    public IReadOnlyList<SessionMember> SessionMembers => _sessionMembers;

    public Guid SessionId => SigningSession.Id;

    public bool AllCommitmentsPublished => _participants.Values.All(x => x.HasCommitment);
    public bool AllNoncesRevealed => _participants.Values.All(x => x.HasReveal);
    public bool AllPartialSignaturesSubmitted => _participants.Values.All(x => x.HasPartialSignature);

    public NPartyProtocolSession(
        Epoch epoch,
        SigningSession signingSession,
        MessageDigestValue messageDigest,
        PublicKeyValue aggregatePublicKey,
        IReadOnlyDictionary<Guid, NPartyParticipantProtocolState> participants,
        IReadOnlyList<SessionMember> sessionMembers,
        SignatureProtectionMode protectionMode = SignatureProtectionMode.Baseline)
    {
        Epoch = epoch ?? throw new ArgumentNullException(nameof(epoch));
        SigningSession = signingSession ?? throw new ArgumentNullException(nameof(signingSession));
        MessageDigest = messageDigest ?? throw new ArgumentNullException(nameof(messageDigest));
        AggregatePublicKey = aggregatePublicKey ?? throw new ArgumentNullException(nameof(aggregatePublicKey));
        _participants = participants ?? throw new ArgumentNullException(nameof(participants));
        _sessionMembers = sessionMembers ?? throw new ArgumentNullException(nameof(sessionMembers));
        ProtectionMode = protectionMode;

        if (_participants.Count < 2)
            throw new ArgumentException("At least two participants are required for an aggregate signing session.", nameof(participants));
    }

    public NPartyParticipantProtocolState GetParticipant(Guid participantId)
    {
        if (!_participants.TryGetValue(participantId, out var participant))
            throw new InvalidOperationException("Participant is not part of this session.");

        return participant;
    }

    public void SetAggregateNoncePoint(PointValue aggregateNoncePoint)
    {
        AggregateNoncePoint = aggregateNoncePoint ?? throw new ArgumentNullException(nameof(aggregateNoncePoint));
    }

    public void SetChallenge(ScalarValue challenge)
    {
        Challenge = challenge ?? throw new ArgumentNullException(nameof(challenge));
    }

    public void SetAggregateSignature(AggregateSignature aggregateSignature)
    {
        AggregateSignature = aggregateSignature ?? throw new ArgumentNullException(nameof(aggregateSignature));
    }
}