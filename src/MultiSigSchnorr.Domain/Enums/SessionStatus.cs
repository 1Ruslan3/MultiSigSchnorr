namespace MultiSigSchnorr.Domain.Enums;

public enum SessionStatus
{
    Created = 0,
    CommitmentsCollection = 1,
    NonceRevealCollection = 2,
    PartialSignaturesCollection = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6
}