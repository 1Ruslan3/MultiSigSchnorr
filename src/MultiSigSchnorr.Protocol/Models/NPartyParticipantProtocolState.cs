using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Protocol.Models;

public sealed class NPartyParticipantProtocolState
{
    public Guid ParticipantId { get; }
    public string DisplayName { get; }
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

    public NPartyParticipantProtocolState(
        Guid participantId,
        string displayName,
        ScalarValue privateKey,
        PublicKeyValue publicKey,
        ScalarValue aggregationCoefficient)
    {
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));

        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(privateKey);
        ArgumentNullException.ThrowIfNull(publicKey);
        ArgumentNullException.ThrowIfNull(aggregationCoefficient);

        ParticipantId = participantId;
        DisplayName = displayName;
        PrivateKey = privateKey;
        PublicKey = publicKey;
        AggregationCoefficient = aggregationCoefficient;
    }

    public NonceCommitment CreateCommitment(
        Guid sessionId,
        DateTime submittedUtc,
        INonceGenerator nonceGenerator,
        ICommitmentService commitmentService)
    {
        ArgumentNullException.ThrowIfNull(nonceGenerator);
        ArgumentNullException.ThrowIfNull(commitmentService);

        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));

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

        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));

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
        ScalarValue challenge,
        SignatureProtectionMode protectionMode = SignatureProtectionMode.Baseline)
    {
        ArgumentNullException.ThrowIfNull(partialSignatureService);
        ArgumentNullException.ThrowIfNull(challenge);

        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));

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
            AggregationCoefficient,
            protectionMode);

        PartialSignatureRecord = new PartialSignature(
            Guid.NewGuid(),
            sessionId,
            ParticipantId,
            scalar,
            submittedUtc);

        return PartialSignatureRecord;
    }
}