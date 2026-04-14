using System.Net.Http.Json;
using MultiSigSchnorr.Contracts.Diagnostics;
using MultiSigSchnorr.Contracts.ProtocolSessions;

namespace MultiSigSchnorr.Web.Services.Api;

public sealed class ProtocolSessionsApiClient
{
    private readonly HttpClient _httpClient;

    public ProtocolSessionsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<DevelopmentSeedApiResponse> GetSeedAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<DevelopmentSeedApiResponse>(
            "api/system/seed",
            cancellationToken);

        return result ?? throw new InvalidOperationException("Seed data response was empty.");
    }

    public async Task<IReadOnlyList<ProtocolSessionHistoryItemApiResponse>> GetSessionHistoryAsync(
    int take = 20,
    CancellationToken cancellationToken = default)
    {
    IReadOnlyList<ProtocolSessionHistoryItemApiResponse>? result =
        await _httpClient.GetFromJsonAsync<List<ProtocolSessionHistoryItemApiResponse>>(
            $"api/protocol-sessions?take={take}",
            cancellationToken);

    return result ?? Array.Empty<ProtocolSessionHistoryItemApiResponse>();
    }

    public async Task<SessionStateApiResponse> CreateProtocolSessionAsync(
        CreateProtocolSessionApiRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/protocol-sessions",
            request,
            cancellationToken);

        return await ReadSessionStateAsync(response, cancellationToken);
    }

    public async Task<SessionStateApiResponse> PublishCommitmentAsync(
        Guid sessionId,
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/protocol-sessions/{sessionId}/commitments",
            new PublishCommitmentApiRequest { ParticipantId = participantId },
            cancellationToken);

        return await ReadSessionStateAsync(response, cancellationToken);
    }

    public async Task<SessionStateApiResponse> RevealNonceAsync(
        Guid sessionId,
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/protocol-sessions/{sessionId}/reveals",
            new RevealNonceApiRequest { ParticipantId = participantId },
            cancellationToken);

        return await ReadSessionStateAsync(response, cancellationToken);
    }

    public async Task<SessionStateApiResponse> SubmitPartialSignatureAsync(
        Guid sessionId,
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/protocol-sessions/{sessionId}/partial-signatures",
            new SubmitPartialSignatureApiRequest { ParticipantId = participantId },
            cancellationToken);

        return await ReadSessionStateAsync(response, cancellationToken);
    }

    public async Task<SessionStateApiResponse> GetSessionStateAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<SessionStateApiResponse>(
            $"api/protocol-sessions/{sessionId}",
            cancellationToken);

        return result ?? throw new InvalidOperationException("Session state response was empty.");
    }

    public async Task<VerifyProtocolSessionSignatureApiResponse> VerifyProtocolSessionSignatureAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/protocol-sessions/{sessionId}/verify",
            new VerifyProtocolSessionSignatureApiRequest { SessionId = sessionId },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"API request failed with status {(int)response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<VerifyProtocolSessionSignatureApiResponse>(
            cancellationToken);

        return result ?? throw new InvalidOperationException("Verify response was empty.");
    }

    private static async Task<SessionStateApiResponse> ReadSessionStateAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"API request failed with status {(int)response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<SessionStateApiResponse>(cancellationToken);

        return result ?? throw new InvalidOperationException("Session state response was empty.");
    }
}