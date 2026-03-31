using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.Constants;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Crypto.Hashing;

namespace MultiSigSchnorr.Crypto.Commitments;

public sealed class CommitmentService : ICommitmentService
{
    private readonly Sha256HashService _hashService;

    public CommitmentService(Sha256HashService hashService)
    {
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
    }

    public CommitmentValue CreateCommitment(PointValue publicNoncePoint)
    {
        ArgumentNullException.ThrowIfNull(publicNoncePoint);

        var hash = _hashService.ComputeHash(
            DomainSeparationTags.NonceCommitment,
            publicNoncePoint.Bytes);

        return new CommitmentValue(hash);
    }

    public bool VerifyCommitment(CommitmentValue commitment, PointValue publicNoncePoint)
    {
        ArgumentNullException.ThrowIfNull(commitment);
        ArgumentNullException.ThrowIfNull(publicNoncePoint);

        var expected = CreateCommitment(publicNoncePoint);
        return expected.Equals(commitment);
    }
}