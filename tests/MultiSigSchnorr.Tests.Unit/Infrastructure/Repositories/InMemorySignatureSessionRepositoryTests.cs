using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Infrastructure.Repositories;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Infrastructure.Repositories;

public sealed class InMemorySignatureSessionRepositoryTests
{
    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_Should_Return_Saved_Session()
    {
        var repository = new InMemorySignatureSessionRepository();

        var session = new SignatureSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new[] { Guid.NewGuid(), Guid.NewGuid() },
            new byte[] { 0x01, 0x02, 0x03 },
            DateTime.UtcNow);

        await repository.AddAsync(session);
        var loaded = await repository.GetByIdAsync(session.Id);

        Assert.NotNull(loaded);
        Assert.Equal(session.Id, loaded!.Id);
        Assert.Equal(session.EpochId, loaded.EpochId);
        Assert.Equal(session.ParticipantIds.Count, loaded.ParticipantIds.Count);
        Assert.Equal(session.Message, loaded.Message);
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Updated_Session_State()
    {
        var repository = new InMemorySignatureSessionRepository();

        var session = new SignatureSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new[] { Guid.NewGuid(), Guid.NewGuid() },
            new byte[] { 0x0A, 0x0B },
            DateTime.UtcNow);

        await repository.AddAsync(session);

        session.FinalizeSession();
        await repository.UpdateAsync(session);

        var loaded = await repository.GetByIdAsync(session.Id);

        Assert.NotNull(loaded);
        Assert.True(loaded!.IsFinalized);
    }

    [Fact]
    public async Task AddAsync_Duplicate_Id_Should_Throw()
    {
        var repository = new InMemorySignatureSessionRepository();

        var session = new SignatureSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new[] { Guid.NewGuid(), Guid.NewGuid() },
            new byte[] { 0xAA },
            DateTime.UtcNow);

        await repository.AddAsync(session);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.AddAsync(session));
    }
}