namespace MultiSigSchnorr.Application.UseCases.GetSessionState;

public sealed class GetSessionStateRequest
{
    public Guid SessionId { get; init; }
}