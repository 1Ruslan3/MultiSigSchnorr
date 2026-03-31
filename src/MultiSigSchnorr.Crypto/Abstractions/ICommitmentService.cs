using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface ICommitmentService
{
    CommitmentValue CreateCommitment(PointValue publicNoncePoint);
    bool VerifyCommitment(CommitmentValue commitment, PointValue publicNoncePoint);
}