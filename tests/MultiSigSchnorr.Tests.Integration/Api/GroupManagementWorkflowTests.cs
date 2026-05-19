using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MultiSigSchnorr.Contracts.Audit;
using MultiSigSchnorr.Contracts.Administration;
using MultiSigSchnorr.Contracts.Diagnostics;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Infrastructure.Persistence;
using MultiSigSchnorr.Infrastructure.Persistence.Entities;

namespace MultiSigSchnorr.Tests.Integration.Api;

public sealed class GroupManagementWorkflowTests : IClassFixture<MultiSigSchnorrApiFactory>
{
    private readonly MultiSigSchnorrApiFactory _factory;

    public GroupManagementWorkflowTests(MultiSigSchnorrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Group_Management_Should_Create_Rename_And_Create_Epoch_With_Selected_Members()
    {
        using var client = _factory.CreateClient();

        var suffix = CreateSuffix();
        var firstName = $"Integration-Alice-{suffix}";
        var secondName = $"Integration-Bob-{suffix}";
        var renamedFirstName = $"Integration-Alice-Renamed-{suffix}";

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = firstName });

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = secondName });

        var stateAfterCreate = await GetAdministrationStateAsync(client);

        var firstParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == firstName);

        var secondParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == secondName);

        Assert.Equal(ParticipantStatus.Active, firstParticipant.ParticipantStatus);
        Assert.Equal(ParticipantStatus.Active, secondParticipant.ParticipantStatus);
        Assert.True(firstParticipant.HasRuntimePrivateKeyMaterial);
        Assert.True(secondParticipant.HasRuntimePrivateKeyMaterial);

        await PutNoContentAsync(
            client,
            $"/api/admin/participants/{firstParticipant.ParticipantId}/display-name",
            new RenameParticipantApiRequest { DisplayName = renamedFirstName });

        var stateAfterRename = await GetAdministrationStateAsync(client);

        var renamedParticipant = Assert.Single(
            stateAfterRename.Participants,
            participant => participant.ParticipantId == firstParticipant.ParticipantId);

        Assert.Equal(renamedFirstName, renamedParticipant.DisplayName);

        var previousActiveEpochId = stateAfterRename.ActiveEpochId;
        var previousActiveEpochNumber = stateAfterRename.ActiveEpochNumber;

        await PostNoContentAsync(
            client,
            "/api/admin/epochs/create-with-members",
            new CreateEpochWithMembersApiRequest
            {
                ParticipantIds = new[]
                {
                    firstParticipant.ParticipantId,
                    secondParticipant.ParticipantId
                }
            });

        var stateAfterEpochCreation = await GetAdministrationStateAsync(client);

        Assert.NotEqual(previousActiveEpochId, stateAfterEpochCreation.ActiveEpochId);
        Assert.True(stateAfterEpochCreation.ActiveEpochNumber > previousActiveEpochNumber);

        var activeMemberIds = stateAfterEpochCreation.Participants
            .Where(participant => participant.IsActiveMemberOfActiveEpoch)
            .Select(participant => participant.ParticipantId)
            .ToHashSet();

        Assert.Contains(firstParticipant.ParticipantId, activeMemberIds);
        Assert.Contains(secondParticipant.ParticipantId, activeMemberIds);
        Assert.Equal(2, activeMemberIds.Count);
    }

    [Fact]
    public async Task System_Seed_Should_Not_Change_Active_Epoch_Composition()
    {
        using var client = _factory.CreateClient();

        var suffix = CreateSuffix();
        var firstName = $"Seed-Stability-Alice-{suffix}";
        var secondName = $"Seed-Stability-Bob-{suffix}";

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = firstName });

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = secondName });

        var stateAfterCreate = await GetAdministrationStateAsync(client);

        var firstParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == firstName);

        var secondParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == secondName);

        await PostNoContentAsync(
            client,
            "/api/admin/epochs/create-with-members",
            new CreateEpochWithMembersApiRequest
            {
                ParticipantIds = new[]
                {
                    firstParticipant.ParticipantId,
                    secondParticipant.ParticipantId
                }
            });

        var stateBeforeSeed = await GetAdministrationStateAsync(client);
        var activeMembersBeforeSeed = GetActiveMemberIds(stateBeforeSeed);

        await GetRequiredAsync<DevelopmentSeedApiResponse>(
            client,
            "/api/system/seed");

        var stateAfterSeed = await GetAdministrationStateAsync(client);
        var activeMembersAfterSeed = GetActiveMemberIds(stateAfterSeed);

        Assert.Equal(stateBeforeSeed.ActiveEpochId, stateAfterSeed.ActiveEpochId);
        Assert.Equal(stateBeforeSeed.ActiveEpochNumber, stateAfterSeed.ActiveEpochNumber);
        Assert.True(activeMembersBeforeSeed.SetEquals(activeMembersAfterSeed));
    }

    [Fact]
    public async Task Revoked_Participant_Should_Not_Be_Active_Member_And_Cannot_Be_Reused_In_New_Epoch()
    {
        using var client = _factory.CreateClient();

        var suffix = CreateSuffix();
        var firstName = $"Revocation-Alice-{suffix}";
        var secondName = $"Revocation-Bob-{suffix}";

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = firstName });

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = secondName });

        var stateAfterCreate = await GetAdministrationStateAsync(client);

        var firstParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == firstName);

        var secondParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == secondName);

        await PostNoContentAsync(
            client,
            "/api/admin/epochs/create-with-members",
            new CreateEpochWithMembersApiRequest
            {
                ParticipantIds = new[]
                {
                    firstParticipant.ParticipantId,
                    secondParticipant.ParticipantId
                }
            });

        await PostOkAsync(
            client,
            $"/api/admin/participants/{firstParticipant.ParticipantId}/revoke",
            new RevokeParticipantApiRequest { Reason = "Integration test revocation" });

        var stateAfterRevoke = await GetAdministrationStateAsync(client);

        var revokedParticipant = Assert.Single(
            stateAfterRevoke.Participants,
            participant => participant.ParticipantId == firstParticipant.ParticipantId);

        Assert.Equal(ParticipantStatus.Revoked, revokedParticipant.ParticipantStatus);
        Assert.True(revokedParticipant.IsMemberOfActiveEpoch);
        Assert.False(revokedParticipant.IsActiveMemberOfActiveEpoch);
        Assert.False(revokedParticipant.CanBeRevoked);

        using var failedResponse = await client.PostAsJsonAsync(
            "/api/admin/epochs/create-with-members",
            new CreateEpochWithMembersApiRequest
            {
                ParticipantIds = new[]
                {
                    firstParticipant.ParticipantId,
                    secondParticipant.ParticipantId
                }
            });

        Assert.Equal(HttpStatusCode.BadRequest, failedResponse.StatusCode);
    }

    [Fact]
    public async Task Participant_Without_Runtime_Private_Key_Should_Not_Be_Allowed_In_New_Epoch()
    {
        using var client = _factory.CreateClient();

        var suffix = CreateSuffix();
        var validParticipantName = $"Runtime-Ready-{suffix}";
        var missingKeyParticipantName = $"Missing-Runtime-Key-{suffix}";

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = validParticipantName });

        var stateAfterCreate = await GetAdministrationStateAsync(client);

        var validParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == validParticipantName);

        Assert.True(validParticipant.HasRuntimePrivateKeyMaterial);

        var seed = await GetRequiredAsync<DevelopmentSeedApiResponse>(
            client,
            "/api/system/seed");

        var publicKeyHex = seed.Participants.First().PublicKeyHex;
        var missingKeyParticipantId = Guid.NewGuid();

        await InsertParticipantWithoutRuntimePrivateKeyAsync(
            missingKeyParticipantId,
            missingKeyParticipantName,
            publicKeyHex);

        var stateWithMissingKeyParticipant = await GetAdministrationStateAsync(client);

        var missingKeyParticipant = Assert.Single(
            stateWithMissingKeyParticipant.Participants,
            participant => participant.ParticipantId == missingKeyParticipantId);

        Assert.Equal(ParticipantStatus.Active, missingKeyParticipant.ParticipantStatus);
        Assert.False(missingKeyParticipant.HasRuntimePrivateKeyMaterial);

        using var failedResponse = await client.PostAsJsonAsync(
            "/api/admin/epochs/create-with-members",
            new CreateEpochWithMembersApiRequest
            {
                ParticipantIds = new[]
                {
                    validParticipant.ParticipantId,
                    missingKeyParticipantId
                }
            });

        Assert.Equal(HttpStatusCode.BadRequest, failedResponse.StatusCode);

        var error = await failedResponse.Content.ReadAsStringAsync();
        Assert.Contains("runtime private key material", error, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public async Task Group_Management_Actions_Should_Be_Audited()
    {
        using var client = _factory.CreateClient();

        var suffix = CreateSuffix();
        var firstName = $"Audit-Alice-{suffix}";
        var secondName = $"Audit-Bob-{suffix}";
        var renamedFirstName = $"Audit-Alice-Renamed-{suffix}";

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = firstName });

        await PostNoContentAsync(
            client,
            "/api/admin/participants",
            new CreateParticipantApiRequest { DisplayName = secondName });

        var stateAfterCreate = await GetAdministrationStateAsync(client);

        var firstParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == firstName);

        var secondParticipant = Assert.Single(
            stateAfterCreate.Participants,
            participant => participant.DisplayName == secondName);

        var creationAudit = await GetAuditLogAsync(
            client,
            $"actionType={AuditActionType.ParticipantCreated}&entityId={firstParticipant.ParticipantId}");

        var participantCreatedEntry = Assert.Single(creationAudit);
        Assert.Equal(AuditActionType.ParticipantCreated, participantCreatedEntry.ActionType);
        Assert.Equal("Participant", participantCreatedEntry.EntityType);
        Assert.Contains(firstName, participantCreatedEntry.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(firstName, participantCreatedEntry.MetadataJson, StringComparison.OrdinalIgnoreCase);

        await PutNoContentAsync(
            client,
            $"/api/admin/participants/{firstParticipant.ParticipantId}/display-name",
            new RenameParticipantApiRequest { DisplayName = renamedFirstName });

        var renameAudit = await GetAuditLogAsync(
            client,
            $"actionType={AuditActionType.ParticipantRenamed}&entityId={firstParticipant.ParticipantId}");

        var participantRenamedEntry = Assert.Single(renameAudit);
        Assert.Equal(AuditActionType.ParticipantRenamed, participantRenamedEntry.ActionType);
        Assert.Contains(firstName, participantRenamedEntry.MetadataJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(renamedFirstName, participantRenamedEntry.MetadataJson, StringComparison.OrdinalIgnoreCase);

        await PostNoContentAsync(
            client,
            "/api/admin/epochs/create-with-members",
            new CreateEpochWithMembersApiRequest
            {
                ParticipantIds = new[]
                {
                    firstParticipant.ParticipantId,
                    secondParticipant.ParticipantId
                }
            });

        var stateAfterEpochCreation = await GetAdministrationStateAsync(client);

        var epochAudit = await GetAuditLogAsync(
            client,
            $"actionType={AuditActionType.EpochCreatedWithMembers}&entityId={stateAfterEpochCreation.ActiveEpochId}");

        var epochCreatedEntry = Assert.Single(epochAudit);
        Assert.Equal(AuditActionType.EpochCreatedWithMembers, epochCreatedEntry.ActionType);
        Assert.Equal("Epoch", epochCreatedEntry.EntityType);
        Assert.Contains(firstParticipant.ParticipantId.ToString(), epochCreatedEntry.MetadataJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(secondParticipant.ParticipantId.ToString(), epochCreatedEntry.MetadataJson, StringComparison.OrdinalIgnoreCase);
    }

    private async Task InsertParticipantWithoutRuntimePrivateKeyAsync(
        Guid participantId,
        string displayName,
        string publicKeyHex)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MultiSigSchnorrDbContext>();

        await dbContext.Participants.AddAsync(new ParticipantEntity
        {
            Id = participantId,
            DisplayName = displayName,
            PublicKeyHex = publicKeyHex,
            Status = ParticipantStatus.Active.ToString(),
            CreatedUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task<EpochAdministrationStateApiResponse> GetAdministrationStateAsync(
        HttpClient client)
    {
        return await GetRequiredAsync<EpochAdministrationStateApiResponse>(
            client,
            "/api/admin/epoch-management");
    }

    private static HashSet<Guid> GetActiveMemberIds(EpochAdministrationStateApiResponse state)
    {
        return state.Participants
            .Where(participant => participant.IsActiveMemberOfActiveEpoch)
            .Select(participant => participant.ParticipantId)
            .ToHashSet();
    }


    private static async Task<IReadOnlyList<AuditLogItemApiResponse>> GetAuditLogAsync(
        HttpClient client,
        string query)
    {
        return await GetRequiredAsync<IReadOnlyList<AuditLogItemApiResponse>>(
            client,
            $"/api/audit?{query}");
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

    private static async Task PostNoContentAsync<TRequest>(
        HttpClient client,
        string url,
        TRequest request)
    {
        using var response = await client.PostAsJsonAsync(url, request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task PutNoContentAsync<TRequest>(
        HttpClient client,
        string url,
        TRequest request)
    {
        using var response = await client.PutAsJsonAsync(url, request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task PostOkAsync<TRequest>(
        HttpClient client,
        string url,
        TRequest request)
    {
        using var response = await client.PostAsJsonAsync(url, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }
}
