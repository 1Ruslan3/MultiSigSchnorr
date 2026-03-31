namespace MultiSigSchnorr.Domain.Constants;

public static class DomainSeparationTags
{
    public const string AggregateKey = "multisig:aggkey";
    public const string NonceCommitment = "multisig:nonce:commitment";
    public const string Challenge = "multisig:challenge";
    public const string PartialSignature = "multisig:partial-signature";
    public const string EpochState = "multisig:epoch-state";
}