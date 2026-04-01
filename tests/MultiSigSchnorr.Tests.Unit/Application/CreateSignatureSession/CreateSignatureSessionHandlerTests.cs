using MultiSigSchnorr.Application.UseCases.CreateSignatureSession;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Application.CreateSignatureSession;

public sealed class CreateSignatureSessionHandlerTests
{
    [Fact]
    public void Handle_Should_Create_Signature_Session_For_Valid_Participants()
    {
        var participant1 = CreateParticipant("Participant-1");
        var participant2 = CreateParticipant("Participant-2");

        var epoch = new Epoch(Guid.NewGuid(), 1, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        var members = new[]
        {
            new EpochMember(Guid.NewGuid(), epoch.Id, participant1.Id, DateTime.UtcNow),
            new EpochMember(Guid.NewGuid(), epoch.Id, participant2.Id, DateTime.UtcNow)
        };

        var handler = new CreateSignatureSessionHandler();

        var request = new CreateSignatureSessionRequest
        {
            EpochId = epoch.Id,
            ParticipantIds = new[] { participant1.Id, participant2.Id },
            Message = new byte[] { 0x01, 0x02, 0x03, 0x04 }
        };

        var session = handler.Handle(
            request,
            epoch,
            new[] { participant1, participant2 },
            members,
            DateTime.UtcNow);

        Assert.Equal(epoch.Id, session.EpochId);
        Assert.Equal(2, session.ParticipantIds.Count);
        Assert.False(session.IsFinalized);
        Assert.Equal(request.Message, session.Message);
    }

    [Fact]
    public void Handle_Should_Throw_When_One_Of_Participants_Is_Not_Active_Epoch_Member()
    {
        var participant1 = CreateParticipant("Participant-1");
        var participant2 = CreateParticipant("Participant-2");

        var epoch = new Epoch(Guid.NewGuid(), 2, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        var members = new[]
        {
            new EpochMember(Guid.NewGuid(), epoch.Id, participant1.Id, DateTime.UtcNow)
        };

        var handler = new CreateSignatureSessionHandler();

        var request = new CreateSignatureSessionRequest
        {
            EpochId = epoch.Id,
            ParticipantIds = new[] { participant1.Id, participant2.Id },
            Message = new byte[] { 0x0A, 0x0B, 0x0C }
        };

        Assert.Throws<InvalidOperationException>(() =>
            handler.Handle(
                request,
                epoch,
                new[] { participant1, participant2 },
                members,
                DateTime.UtcNow));
    }

    [Fact]
    public void Handle_Should_Throw_When_Epoch_Is_Not_Active()
    {
        var participant1 = CreateParticipant("Participant-1");
        var participant2 = CreateParticipant("Participant-2");

        var epoch = new Epoch(Guid.NewGuid(), 3, DateTime.UtcNow);

        var members = new[]
        {
            new EpochMember(Guid.NewGuid(), epoch.Id, participant1.Id, DateTime.UtcNow),
            new EpochMember(Guid.NewGuid(), epoch.Id, participant2.Id, DateTime.UtcNow)
        };

        var handler = new CreateSignatureSessionHandler();

        var request = new CreateSignatureSessionRequest
        {
            EpochId = epoch.Id,
            ParticipantIds = new[] { participant1.Id, participant2.Id },
            Message = new byte[] { 0xAA, 0xBB }
        };

        Assert.Throws<InvalidOperationException>(() =>
            handler.Handle(
                request,
                epoch,
                new[] { participant1, participant2 },
                members,
                DateTime.UtcNow));
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