using System.Net.Http.Json;
using MultiSigSchnorr.Contracts.Administration;
using MultiSigSchnorr.Contracts.Audit;
using MultiSigSchnorr.Contracts.Diagnostics;
using MultiSigSchnorr.Contracts.ProtocolSessions;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Web.Services.Api;

public sealed class ProtocolSessionsApiClient
{
    private readonly HttpClient _httpClient;

    public ProtocolSessionsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Uri BaseAddress => _httpClient.BaseAddress
        ?? throw new InvalidOperationException("API base address is not configured.");

    public string BuildAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Relative path cannot be empty.", nameof(relativePath));

        return new Uri(BaseAddress, relativePath).ToString();
    }

    public async Task<DevelopmentSeedApiResponse> GetSeedAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<DevelopmentSeedApiResponse>(
            "api/system/seed",
            cancellationToken);

        return result ?? throw new InvalidOperationException("Seed data response was empty.");
    }

    public async Task<StorageDiagnosticsApiResponse> GetStorageDiagnosticsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<StorageDiagnosticsApiResponse>(
            "api/system/storage",
            cancellationToken);

        return result ?? throw new InvalidOperationException("Storage diagnostics response was empty.");
    }

    public async Task<EpochAdministrationStateApiResponse> GetAdministrationStateAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<EpochAdministrationStateApiResponse>(
            "api/admin/epoch-management",
            cancellationToken);

        return result ?? throw new InvalidOperationException("Administration state response was empty.");
    }

    public async Task<IReadOnlyList<AuditLogItemApiResponse>> GetAuditLogAsync(
        int take = 100,
        string? search = null,
        AuditActionType? actionType = null,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>
        {
            $"take={take}"
        };

        if (!string.IsNullOrWhiteSpace(search))
            queryParts.Add($"search={Uri.EscapeDataString(search)}");

        if (actionType.HasValue)
            queryParts.Add($"actionType={Uri.EscapeDataString(actionType.Value.ToString())}");

        if (!string.IsNullOrWhiteSpace(entityType))
            queryParts.Add($"entityType={Uri.EscapeDataString(entityType)}");

        if (entityId.HasValue)
            queryParts.Add($"entityId={Uri.EscapeDataString(entityId.Value.ToString())}");

        var path = $"api/audit?{string.Join("&", queryParts)}";

        IReadOnlyList<AuditLogItemApiResponse>? result =
            await _httpClient.GetFromJsonAsync<List<AuditLogItemApiResponse>>(
                path,
                cancellationToken);

        return result ?? Array.Empty<AuditLogItemApiResponse>();
    }

    public async Task<EpochAdministrationStateApiResponse> RevokeParticipantAsync(
        Guid participantId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/admin/participants/{participantId}/revoke",
            new RevokeParticipantApiRequest { Reason = reason },
            cancellationToken);

        return await ReadAdministrationStateAsync(response, cancellationToken);
    }

    public async Task<EpochAdministrationStateApiResponse> TransitionToNextEpochAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            "api/admin/epochs/transition",
            content: null,
            cancellationToken);

        return await ReadAdministrationStateAsync(response, cancellationToken);
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

    public async Task<ProtocolSessionReportApiResponse> GetSessionReportAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<ProtocolSessionReportApiResponse>(
            $"api/protocol-sessions/{sessionId}/report",
            cancellationToken);

        return result ?? throw new InvalidOperationException("Session report response was empty.");
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

    private static async Task<EpochAdministrationStateApiResponse> ReadAdministrationStateAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"API request failed with status {(int)response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<EpochAdministrationStateApiResponse>(cancellationToken);

        return result ?? throw new InvalidOperationException("Administration state response was empty.");
    }
}