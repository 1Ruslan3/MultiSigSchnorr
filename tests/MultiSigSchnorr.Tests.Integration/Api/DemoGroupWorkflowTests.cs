using System.Net;
using System.Net.Http.Json;
using MultiSigSchnorr.Contracts.Administration;
using MultiSigSchnorr.Contracts.Audit;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Tests.Integration.Api;

public sealed class DemoGroupWorkflowTests : IClassFixture<MultiSigSchnorrApiFactory>
{
    private readonly MultiSigSchnorrApiFactory _factory;

    public DemoGroupWorkflowTests(MultiSigSchnorrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Demo_Group_Should_Create_Runtime_Ready_Participants_And_New_Active_Epoch()
    {
        using var client = _factory.CreateClient();

        var before = await GetAdministrationStateAsync(client);
        var prefix = $"Demo-Test-{Guid.NewGuid():N}"[..18];

        await PostNoContentAsync(
            client,
            "/api/admin/demo-group",
            new CreateDemoGroupApiRequest
            {
                ParticipantsCount = 3,
                DisplayNamePrefix = prefix
            });

        var after = await GetAdministrationStateAsync(client);

        Assert.NotEqual(before.ActiveEpochId, after.ActiveEpochId);
        Assert.True(after.ActiveEpochNumber > before.ActiveEpochNumber);

        var activeMembers = after.Participants
            .Where(x => x.IsActiveMemberOfActiveEpoch)
            .ToList();

        Assert.Equal(3, activeMembers.Count);
        Assert.All(activeMembers, participant =>
        {
            Assert.Equal(ParticipantStatus.Active, participant.ParticipantStatus);
            Assert.True(participant.HasRuntimePrivateKeyMaterial);
            Assert.StartsWith(prefix, participant.DisplayName, StringComparison.Ordinal);
        });

        var audit = await GetRequiredAsync<IReadOnlyList<AuditLogItemApiResponse>>(
            client,
            $"/api/audit?actionType={AuditActionType.DemoGroupCreated}&entityId={after.ActiveEpochId}");

        var auditEntry = Assert.Single(audit);
        Assert.Equal(AuditActionType.DemoGroupCreated, auditEntry.ActionType);
        Assert.Equal("Epoch", auditEntry.EntityType);
        Assert.Contains(prefix, auditEntry.MetadataJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(after.ActiveEpochId.ToString(), auditEntry.MetadataJson, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<EpochAdministrationStateApiResponse> GetAdministrationStateAsync(
        HttpClient client)
    {
        return await GetRequiredAsync<EpochAdministrationStateApiResponse>(
            client,
            "/api/admin/epoch-management");
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
}
