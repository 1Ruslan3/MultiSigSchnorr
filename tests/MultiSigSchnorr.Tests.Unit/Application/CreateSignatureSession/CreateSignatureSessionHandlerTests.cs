using MultiSigSchnorr.Application.UseCases.CreateSignatureSession;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Infrastructure.Repositories;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Application.CreateSignatureSession;

public sealed class CreateSignatureSessionHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Create_And_Save_Signature_Session_For_Valid_Participants()
    {
        var epochRepository = new InMemoryEpochRepository();
        var participantRepository = new InMemoryParticipantRepository();
        var epochMemberRepository = new InMemoryEpochMemberRepository();
        var signatureSessionRepository = new InMemorySignatureSessionRepository();

        var participant1 = CreateParticipant("Participant-1");
        var participant2 = CreateParticipant("Participant-2");

        await participantRepository.AddAsync(participant1);
        await participantRepository.AddAsync(participant2);

        var epoch = new Epoch(Guid.NewGuid(), 1, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        await epochRepository.AddAsync(epoch);

        await epochMemberRepository.AddAsync(
            new EpochMember(Guid.NewGuid(), epoch.Id, participant1.Id, DateTime.UtcNow));

        await epochMemberRepository.AddAsync(
            new EpochMember(Guid.NewGuid(), epoch.Id, participant2.Id, DateTime.UtcNow));

        var handler = new CreateSignatureSessionHandler(
            epochRepository,
            participantRepository,
            epochMemberRepository,
            signatureSessionRepository);

        var request = new CreateSignatureSessionRequest
        {
            EpochId = epoch.Id,
            ParticipantIds = new[] { participant1.Id, participant2.Id },
            Message = new byte[] { 0x01, 0x02, 0x03, 0x04 }
        };

        var session = await handler.HandleAsync(request, DateTime.UtcNow);

        var loaded = await signatureSessionRepository.GetByIdAsync(session.Id);

        Assert.Equal(epoch.Id, session.EpochId);
        Assert.Equal(2, session.ParticipantIds.Count);
        Assert.False(session.IsFinalized);

        Assert.NotNull(loaded);
        Assert.Equal(session.Id, loaded!.Id);
        Assert.Equal(session.EpochId, loaded.EpochId);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_One_Of_Participants_Is_Not_Active_Epoch_Member()
    {
        var epochRepository = new InMemoryEpochRepository();
        var participantRepository = new InMemoryParticipantRepository();
        var epochMemberRepository = new InMemoryEpochMemberRepository();
        var signatureSessionRepository = new InMemorySignatureSessionRepository();

        var participant1 = CreateParticipant("Participant-1");
        var participant2 = CreateParticipant("Participant-2");

        await participantRepository.AddAsync(participant1);
        await participantRepository.AddAsync(participant2);

        var epoch = new Epoch(Guid.NewGuid(), 2, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        await epochRepository.AddAsync(epoch);

        await epochMemberRepository.AddAsync(
            new EpochMember(Guid.NewGuid(), epoch.Id, participant1.Id, DateTime.UtcNow));

        var handler = new CreateSignatureSessionHandler(
            epochRepository,
            participantRepository,
            epochMemberRepository,
            signatureSessionRepository);

        var request = new CreateSignatureSessionRequest
        {
            EpochId = epoch.Id,
            ParticipantIds = new[] { participant1.Id, participant2.Id },
            Message = new byte[] { 0x0A, 0x0B, 0x0C }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, DateTime.UtcNow));
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_Epoch_Is_Not_Active()
    {
        var epochRepository = new InMemoryEpochRepository();
        var participantRepository = new InMemoryParticipantRepository();
        var epochMemberRepository = new InMemoryEpochMemberRepository();
        var signatureSessionRepository = new InMemorySignatureSessionRepository();

        var participant1 = CreateParticipant("Participant-1");
        var participant2 = CreateParticipant("Participant-2");

        await participantRepository.AddAsync(participant1);
        await participantRepository.AddAsync(participant2);

        var epoch = new Epoch(Guid.NewGuid(), 3, DateTime.UtcNow);
        await epochRepository.AddAsync(epoch);

        await epochMemberRepository.AddAsync(
            new EpochMember(Guid.NewGuid(), epoch.Id, participant1.Id, DateTime.UtcNow));

        await epochMemberRepository.AddAsync(
            new EpochMember(Guid.NewGuid(), epoch.Id, participant2.Id, DateTime.UtcNow));

        var handler = new CreateSignatureSessionHandler(
            epochRepository,
            participantRepository,
            epochMemberRepository,
            signatureSessionRepository);

        var request = new CreateSignatureSessionRequest
        {
            EpochId = epoch.Id,
            ParticipantIds = new[] { participant1.Id, participant2.Id },
            Message = new byte[] { 0xAA, 0xBB }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, DateTime.UtcNow));
    }

    private static Participant CreateParticipant(string name)
    {
        return new Participant(
            Guid.NewGuid(),
            name,
            new PublicKeyValue(new PointValue(new byte[] { 0x04, 0x01, 0x02, 0x03, 0x04 })),
            ParticipantStatus.Active,
            DateTime.UtcNow);
    }
}