using System.Net;
using System.Net.Http.Json;
using MultiSigSchnorr.Contracts.Diagnostics;
using MultiSigSchnorr.Contracts.ProtocolSessions;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Tests.Integration.Api;

public sealed class ProtocolSessionsWorkflowTests : IClassFixture<MultiSigSchnorrApiFactory>
{
    private readonly MultiSigSchnorrApiFactory _factory;

    public ProtocolSessionsWorkflowTests(MultiSigSchnorrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Full_Workflow_Should_Create_Complete_Verify_And_Export_Report_In_Randomized_Mode()
    {
        using var client = _factory.CreateClient();

        var seed = await GetRequiredAsync<DevelopmentSeedApiResponse>(
            client,
            "/api/system/seed");

        var createdSession = await PostRequiredAsync<CreateProtocolSessionApiRequest, SessionStateApiResponse>(
            client,
            "/api/protocol-sessions",
            new CreateProtocolSessionApiRequest
            {
                EpochId = seed.EpochId,
                ParticipantIds = seed.ParticipantIds,
                Message = "integration-test-session",
                ProtectionMode = SignatureProtectionMode.RandomizedScalarProcessing
            });

        Assert.NotEqual(Guid.Empty, createdSession.SessionId);
        Assert.Equal(SessionStatus.Created, createdSession.SessionStatus);
        Assert.Equal(SignatureProtectionMode.RandomizedScalarProcessing, createdSession.ProtectionMode);

        foreach (var participantId in seed.ParticipantIds)
        {
            createdSession = await PostRequiredAsync<PublishCommitmentApiRequest, SessionStateApiResponse>(
                client,
                $"/api/protocol-sessions/{createdSession.SessionId}/commitments",
                new PublishCommitmentApiRequest
                {
                    ParticipantId = participantId
                });
        }

        Assert.True(createdSession.AllCommitmentsPublished);
        Assert.Equal(SessionStatus.NonceRevealCollection, createdSession.SessionStatus);

        foreach (var participantId in seed.ParticipantIds)
        {
            createdSession = await PostRequiredAsync<RevealNonceApiRequest, SessionStateApiResponse>(
                client,
                $"/api/protocol-sessions/{createdSession.SessionId}/reveals",
                new RevealNonceApiRequest
                {
                    ParticipantId = participantId
                });
        }

        Assert.True(createdSession.AllNoncesRevealed);
        Assert.Equal(SessionStatus.PartialSignaturesCollection, createdSession.SessionStatus);

        foreach (var participantId in seed.ParticipantIds)
        {
            createdSession = await PostRequiredAsync<SubmitPartialSignatureApiRequest, SessionStateApiResponse>(
                client,
                $"/api/protocol-sessions/{createdSession.SessionId}/partial-signatures",
                new SubmitPartialSignatureApiRequest
                {
                    ParticipantId = participantId
                });
        }

        Assert.True(createdSession.AllPartialSignaturesSubmitted);
        Assert.Equal(SessionStatus.Completed, createdSession.SessionStatus);
        Assert.False(string.IsNullOrWhiteSpace(createdSession.AggregateSignatureScalarHex));
        Assert.Equal(SignatureProtectionMode.RandomizedScalarProcessing, createdSession.ProtectionMode);

        var verification = await PostRequiredAsync<VerifyProtocolSessionSignatureApiRequest, VerifyProtocolSessionSignatureApiResponse>(
            client,
            $"/api/protocol-sessions/{createdSession.SessionId}/verify",
            new VerifyProtocolSessionSignatureApiRequest
            {
                SessionId = createdSession.SessionId
            });

        Assert.True(verification.IsValid);
        Assert.Equal("Aggregate signature is valid.", verification.Message);

        var report = await GetRequiredAsync<ProtocolSessionReportApiResponse>(
            client,
            $"/api/protocol-sessions/{createdSession.SessionId}/report");

        Assert.Equal(createdSession.SessionId, report.SessionId);
        Assert.Equal(SessionStatus.Completed, report.SessionStatus);
        Assert.Equal(SignatureProtectionMode.RandomizedScalarProcessing, report.ProtectionMode);
        Assert.True(report.AllCommitmentsPublished);
        Assert.True(report.AllNoncesRevealed);
        Assert.True(report.AllPartialSignaturesSubmitted);
        Assert.Equal(seed.ParticipantIds.Count, report.Participants.Count);

        using var jsonFileResponse = await client.GetAsync(
            $"/api/protocol-sessions/{createdSession.SessionId}/report.json");

        Assert.Equal(HttpStatusCode.OK, jsonFileResponse.StatusCode);
        Assert.Equal("application/json", jsonFileResponse.Content.Headers.ContentType?.MediaType);

        var jsonFileContent = await jsonFileResponse.Content.ReadAsStringAsync();
        Assert.Contains(createdSession.SessionId.ToString(), jsonFileContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("aggregateSignatureScalarHex", jsonFileContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("protectionMode", jsonFileContent, StringComparison.OrdinalIgnoreCase);

        using var textFileResponse = await client.GetAsync(
            $"/api/protocol-sessions/{createdSession.SessionId}/report.txt");

        Assert.Equal(HttpStatusCode.OK, textFileResponse.StatusCode);
        Assert.Equal("text/plain", textFileResponse.Content.Headers.ContentType?.MediaType);

        var textFileContent = await textFileResponse.Content.ReadAsStringAsync();
        Assert.Contains("MULTISIG SCHNORR PROTOCOL SESSION REPORT", textFileContent, StringComparison.Ordinal);
        Assert.Contains(createdSession.SessionId.ToString(), textFileContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Protection Mode: RandomizedScalarProcessing", textFileContent, StringComparison.Ordinal);

        var finalState = await GetRequiredAsync<SessionStateApiResponse>(
            client,
            $"/api/protocol-sessions/{createdSession.SessionId}");

        Assert.Equal(SessionStatus.Completed, finalState.SessionStatus);
        Assert.Equal(SignatureProtectionMode.RandomizedScalarProcessing, finalState.ProtectionMode);
        Assert.True(finalState.AllCommitmentsPublished);
        Assert.True(finalState.AllNoncesRevealed);
        Assert.True(finalState.AllPartialSignaturesSubmitted);
        Assert.All(finalState.Participants, participant => Assert.True(participant.HasPartialSignature));
    }

    [Fact]
    public async Task History_Endpoint_Should_Contain_Created_Session_With_Protection_Mode()
    {
        using var client = _factory.CreateClient();

        var seed = await GetRequiredAsync<DevelopmentSeedApiResponse>(
            client,
            "/api/system/seed");

        var createdSession = await PostRequiredAsync<CreateProtocolSessionApiRequest, SessionStateApiResponse>(
            client,
            "/api/protocol-sessions",
            new CreateProtocolSessionApiRequest
            {
                EpochId = seed.EpochId,
                ParticipantIds = seed.ParticipantIds,
                Message = "history-test-session",
                ProtectionMode = SignatureProtectionMode.Baseline
            });

        var history = await GetRequiredAsync<List<ProtocolSessionHistoryItemApiResponse>>(
            client,
            "/api/protocol-sessions?take=50");

        var entry = Assert.Single(history.Where(item => item.SessionId == createdSession.SessionId));
        Assert.Equal(SignatureProtectionMode.Baseline, entry.ProtectionMode);
    }

    private static async Task<TResponse> GetRequiredAsync<TResponse>(
        HttpClient client,
        string url)
    {
        using var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<TResponse>();
        Assert.NotNull(payload);

        return payload!;
    }

    private static async Task<TResponse> PostRequiredAsync<TRequest, TResponse>(
        HttpClient client,
        string url,
        TRequest request)
    {
        using var response = await client.PostAsJsonAsync(url, request);

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created,
            $"Unexpected status code {(int)response.StatusCode} for '{url}'.");

        var payload = await response.Content.ReadFromJsonAsync<TResponse>();
        Assert.NotNull(payload);

        return payload!;
    }
}