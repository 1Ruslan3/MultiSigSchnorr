using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Protocol.Epochs;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Protocol.Epochs;

public sealed class EpochTransitionServiceTests
{
    [Fact]
    public void TransitionToNextEpoch_Should_Close_Current_And_Carry_Only_Active_Participants()
    {
        var p1 = new Participant(
            Guid.NewGuid(),
            "Participant-1",
            new PublicKeyValue(new PointValue(new byte[] { 0x04, 0x11, 0x12, 0x13 })),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var p2 = new Participant(
            Guid.NewGuid(),
            "Participant-2",
            new PublicKeyValue(new PointValue(new byte[] { 0x04, 0x21, 0x22, 0x23 })),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var p3 = new Participant(
            Guid.NewGuid(),
            "Participant-3",
            new PublicKeyValue(new PointValue(new byte[] { 0x04, 0x31, 0x32, 0x33 })),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        p3.Revoke(DateTime.UtcNow);

        var currentEpoch = new Epoch(Guid.NewGuid(), 5, DateTime.UtcNow);
        currentEpoch.Activate(DateTime.UtcNow);

        var members = new List<EpochMember>
        {
            new(Guid.NewGuid(), currentEpoch.Id, p1.Id, DateTime.UtcNow),
            new(Guid.NewGuid(), currentEpoch.Id, p2.Id, DateTime.UtcNow),
            new(Guid.NewGuid(), currentEpoch.Id, p3.Id, DateTime.UtcNow)
        };

        var service = new EpochTransitionService();

        var result = service.TransitionToNextEpoch(
            currentEpoch,
            new[] { p1, p2, p3 },
            members,
            DateTime.UtcNow);

        Assert.Equal(EpochStatus.Closed, result.PreviousEpoch.Status);
        Assert.Equal(EpochStatus.Active, result.NewEpoch.Status);
        Assert.Equal(6, result.NewEpoch.Number);
        Assert.Equal(2, result.NewEpochMembers.Count);
        Assert.DoesNotContain(result.NewEpochMembers, x => x.ParticipantId == p3.Id);
        Assert.All(result.NewEpochMembers, x => Assert.Equal(result.NewEpoch.Id, x.EpochId));
    }

    [Fact]
    public void TransitionToNextEpoch_With_Less_Than_Two_Active_Participants_Should_Throw()
    {
        var p1 = new Participant(
            Guid.NewGuid(),
            "Participant-1",
            new PublicKeyValue(new PointValue(new byte[] { 0x04, 0x41, 0x42, 0x43 })),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var p2 = new Participant(
            Guid.NewGuid(),
            "Participant-2",
            new PublicKeyValue(new PointValue(new byte[] { 0x04, 0x51, 0x52, 0x53 })),
            ParticipantStatus.Revoked,
            DateTime.UtcNow);

        var currentEpoch = new Epoch(Guid.NewGuid(), 9, DateTime.UtcNow);
        currentEpoch.Activate(DateTime.UtcNow);

        var members = new List<EpochMember>
        {
            new(Guid.NewGuid(), currentEpoch.Id, p1.Id, DateTime.UtcNow),
            new(Guid.NewGuid(), currentEpoch.Id, p2.Id, DateTime.UtcNow)
        };

        var service = new EpochTransitionService();

        Assert.Throws<InvalidOperationException>(() =>
            service.TransitionToNextEpoch(
                currentEpoch,
                new[] { p1, p2 },
                members,
                DateTime.UtcNow));
    }
}