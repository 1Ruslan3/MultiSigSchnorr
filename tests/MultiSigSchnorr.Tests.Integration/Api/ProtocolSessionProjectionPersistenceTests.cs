using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiSigSchnorr.Contracts.Diagnostics;
using MultiSigSchnorr.Contracts.ProtocolSessions;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Infrastructure.Persistence;

namespace MultiSigSchnorr.Tests.Integration.Api;

public sealed class ProtocolSessionProjectionPersistenceTests
    : IClassFixture<MultiSigSchnorrApiFactory>
{
    private readonly MultiSigSchnorrApiFactory _factory;

    public ProtocolSessionProjectionPersistenceTests(MultiSigSchnorrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Completed_ProtocolSession_Should_Be_Persisted_As_Postgres_Projection()
    {
        using var client = _factory.CreateClient();

        var seed = await GetRequiredAsync<DevelopmentSeedApiResponse>(
            client,
            "/api/system/seed");

        var session = await PostRequiredAsync<CreateProtocolSessionApiRequest, SessionStateApiResponse>(
            client,
            "/api/protocol-sessions",
            new CreateProtocolSessionApiRequest
            {
                EpochId = seed.EpochId,
                ParticipantIds = seed.ParticipantIds,
                Message = "postgres-projection-integration-test",
                ProtectionMode = SignatureProtectionMode.RandomizedScalarProcessing
            });

        foreach (var participantId in seed.ParticipantIds)
        {
            session = await PostRequiredAsync<PublishCommitmentApiRequest, SessionStateApiResponse>(
                client,
                $"/api/protocol-sessions/{session.SessionId}/commitments",
                new PublishCommitmentApiRequest
                {
                    ParticipantId = participantId
                });
        }

        foreach (var participantId in seed.ParticipantIds)
        {
            session = await PostRequiredAsync<RevealNonceApiRequest, SessionStateApiResponse>(
                client,
                $"/api/protocol-sessions/{session.SessionId}/reveals",
                new RevealNonceApiRequest
                {
                    ParticipantId = participantId
                });
        }

        foreach (var participantId in seed.ParticipantIds)
        {
            session = await PostRequiredAsync<SubmitPartialSignatureApiRequest, SessionStateApiResponse>(
                client,
                $"/api/protocol-sessions/{session.SessionId}/partial-signatures",
                new SubmitPartialSignatureApiRequest
                {
                    ParticipantId = participantId
                });
        }

        Assert.Equal(SessionStatus.Completed, session.SessionStatus);
        Assert.True(session.AllCommitmentsPublished);
        Assert.True(session.AllNoncesRevealed);
        Assert.True(session.AllPartialSignaturesSubmitted);
        Assert.False(string.IsNullOrWhiteSpace(session.AggregateSignatureScalarHex));

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MultiSigSchnorrDbContext>();

        var storedSession = await dbContext.ProtocolSessions
            .AsNoTracking()
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.SessionId == session.SessionId);

        Assert.NotNull(storedSession);

        Assert.Equal(session.SessionId, storedSession.SessionId);
        Assert.Equal(seed.EpochId, storedSession.EpochId);
        Assert.Equal(SessionStatus.Completed.ToString(), storedSession.SessionStatus);
        Assert.Equal(SignatureProtectionMode.RandomizedScalarProcessing.ToString(), storedSession.ProtectionMode);
        Assert.Equal(session.MessageDigestHex, storedSession.MessageDigestHex);
        Assert.Equal(session.AggregatePublicKeyHex, storedSession.AggregatePublicKeyHex);
        Assert.Equal(session.AggregateNoncePointHex, storedSession.AggregateNoncePointHex);
        Assert.Equal(session.ChallengeHex, storedSession.ChallengeHex);
        Assert.Equal(session.AggregateSignatureScalarHex, storedSession.AggregateSignatureScalarHex);

        Assert.True(storedSession.AllCommitmentsPublished);
        Assert.True(storedSession.AllNoncesRevealed);
        Assert.True(storedSession.AllPartialSignaturesSubmitted);

        Assert.Equal(seed.ParticipantIds.Count, storedSession.Participants.Count);

        foreach (var participant in storedSession.Participants)
        {
            Assert.True(participant.HasCommitment);
            Assert.True(participant.HasReveal);
            Assert.True(participant.HasPartialSignature);

            Assert.False(string.IsNullOrWhiteSpace(participant.PublicKeyHex));
            Assert.False(string.IsNullOrWhiteSpace(participant.AggregationCoefficientHex));
            Assert.False(string.IsNullOrWhiteSpace(participant.CommitmentHex));
            Assert.False(string.IsNullOrWhiteSpace(participant.PublicNoncePointHex));
            Assert.False(string.IsNullOrWhiteSpace(participant.PartialSignatureHex));
        }
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