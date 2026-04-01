using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Protocol.Models;

public sealed class TwoPartyParticipantProtocolState
{
    public Guid ParticipantId { get; }
    public ScalarValue PrivateKey { get; }
    public PublicKeyValue PublicKey { get; }
    public ScalarValue AggregationCoefficient { get; }

    public ScalarValue? SecretNonce { get; private set; }
    public PointValue? PreparedPublicNoncePoint { get; private set; }

    public NonceCommitment? CommitmentRecord { get; private set; }
    public NonceReveal? RevealRecord { get; private set; }
    public PartialSignature? PartialSignatureRecord { get; private set; }

    public bool HasCommitment => CommitmentRecord is not null;
    public bool HasReveal => RevealRecord is not null;
    public bool HasPartialSignature => PartialSignatureRecord is not null;

    public TwoPartyParticipantProtocolState(
        Guid participantId,
        ScalarValue privateKey,
        PublicKeyValue publicKey,
        ScalarValue aggregationCoefficient)
    {
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));

        ParticipantId = participantId;
        PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        AggregationCoefficient = aggregationCoefficient ?? throw new ArgumentNullException(nameof(aggregationCoefficient));
    }

    public NonceCommitment CreateCommitment(
        Guid sessionId,
        DateTime submittedUtc,
        INonceGenerator nonceGenerator,
        ICommitmentService commitmentService)
    {
        ArgumentNullException.ThrowIfNull(nonceGenerator);
        ArgumentNullException.ThrowIfNull(commitmentService);

        if (HasCommitment)
            throw new InvalidOperationException("Commitment has already been created for this participant.");

        SecretNonce = nonceGenerator.GenerateNonce();
        PreparedPublicNoncePoint = nonceGenerator.CreatePublicNonce(SecretNonce);

        var commitmentValue = commitmentService.CreateCommitment(PreparedPublicNoncePoint);

        CommitmentRecord = new NonceCommitment(
            Guid.NewGuid(),
            sessionId,
            ParticipantId,
            commitmentValue,
            submittedUtc);

        return CommitmentRecord;
    }

    public NonceReveal RevealNonce(
        Guid sessionId,
        DateTime submittedUtc,
        ICommitmentService commitmentService)
    {
        ArgumentNullException.ThrowIfNull(commitmentService);

        if (!HasCommitment)
            throw new InvalidOperationException("Commitment must be created before nonce reveal.");

        if (HasReveal)
            throw new InvalidOperationException("Nonce has already been revealed for this participant.");

        if (SecretNonce is null || PreparedPublicNoncePoint is null || CommitmentRecord is null)
            throw new InvalidOperationException("Nonce preparation state is incomplete.");

        if (!commitmentService.VerifyCommitment(CommitmentRecord.Commitment, PreparedPublicNoncePoint))
            throw new InvalidOperationException("Stored commitment does not match the prepared public nonce point.");

        RevealRecord = new NonceReveal(
            Guid.NewGuid(),
            sessionId,
            ParticipantId,
            PreparedPublicNoncePoint,
            submittedUtc);

        return RevealRecord;
    }

    public PartialSignature CreatePartialSignature(
        Guid sessionId,
        DateTime submittedUtc,
        IPartialSignatureService partialSignatureService,
        ScalarValue challenge)
    {
        ArgumentNullException.ThrowIfNull(partialSignatureService);
        ArgumentNullException.ThrowIfNull(challenge);

        if (!HasReveal)
            throw new InvalidOperationException("Nonce must be revealed before partial signature creation.");

        if (HasPartialSignature)
            throw new InvalidOperationException("Partial signature has already been created for this participant.");

        if (SecretNonce is null)
            throw new InvalidOperationException("Secret nonce is not available.");

        var scalar = partialSignatureService.CreatePartialSignature(
            SecretNonce,
            PrivateKey,
            challenge,
            AggregationCoefficient);

        PartialSignatureRecord = new PartialSignature(
            Guid.NewGuid(),
            sessionId,
            ParticipantId,
            scalar,
            submittedUtc);

        return PartialSignatureRecord;
    }
}