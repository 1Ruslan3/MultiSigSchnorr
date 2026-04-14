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
    public async Task Full_Workflow_Should_Create_Complete_And_Verify_Aggregate_Signature()
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
                Message = "integration-test-session"
            });

        Assert.NotEqual(Guid.Empty, createdSession.SessionId);
        Assert.Equal(SessionStatus.Created, createdSession.SessionStatus);

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

        var verification = await PostRequiredAsync<VerifyProtocolSessionSignatureApiRequest, VerifyProtocolSessionSignatureApiResponse>(
            client,
            $"/api/protocol-sessions/{createdSession.SessionId}/verify",
            new VerifyProtocolSessionSignatureApiRequest
            {
                SessionId = createdSession.SessionId
            });

        Assert.True(verification.IsValid);
        Assert.Equal("Aggregate signature is valid.", verification.Message);

        var finalState = await GetRequiredAsync<SessionStateApiResponse>(
            client,
            $"/api/protocol-sessions/{createdSession.SessionId}");

        Assert.Equal(SessionStatus.Completed, finalState.SessionStatus);
        Assert.True(finalState.AllCommitmentsPublished);
        Assert.True(finalState.AllNoncesRevealed);
        Assert.True(finalState.AllPartialSignaturesSubmitted);
        Assert.All(finalState.Participants, participant => Assert.True(participant.HasPartialSignature));
    }

    [Fact]
    public async Task History_Endpoint_Should_Contain_Created_Session()
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
                Message = "history-test-session"
            });

        var history = await GetRequiredAsync<List<ProtocolSessionHistoryItemApiResponse>>(
            client,
            "/api/protocol-sessions?take=50");

        Assert.Contains(history, item => item.SessionId == createdSession.SessionId);
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