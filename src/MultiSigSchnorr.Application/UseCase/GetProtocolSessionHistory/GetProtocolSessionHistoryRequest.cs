namespace MultiSigSchnorr.Application.UseCases.GetProtocolSessionHistory;

public sealed class GetProtocolSessionHistoryRequest
{
    public int Take { get; init; } = 20;
}