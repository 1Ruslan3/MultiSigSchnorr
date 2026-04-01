using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Protocol.Revocation;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Protocol.Revocation;

public sealed class RevocationPolicyServiceTests
{
    [Fact]
    public void RevokeParticipant_Should_Revoke_Participant_And_Deactivate_Epoch_Membership()
    {
        var publicKey = new PublicKeyValue(new PointValue(new byte[] { 0x04, 0x01, 0x02, 0x03 }));
        var participant = new Participant(
            Guid.NewGuid(),
            "Participant-1",
            publicKey,
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var epoch = new Epoch(Guid.NewGuid(), 1, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        var epochMember = new EpochMember(
            Guid.NewGuid(),
            epoch.Id,
            participant.Id,
            DateTime.UtcNow);

        var service = new RevocationPolicyService();

        var result = service.RevokeParticipant(
            epoch,
            participant,
            new[] { epochMember },
            "Manual revocation for security reasons",
            DateTime.UtcNow);

        Assert.Equal(ParticipantStatus.Revoked, participant.Status);
        Assert.False(epochMember.IsActive);
        Assert.Equal(participant.Id, result.RevocationRecord.ParticipantId);
        Assert.Equal(epoch.Id, result.RevocationRecord.EpochId);
        Assert.Single(result.DeactivatedMembers);
    }

    [Fact]
    public void RevokeParticipant_In_Inactive_Epoch_Should_Throw()
    {
        var publicKey = new PublicKeyValue(new PointValue(new byte[] { 0x04, 0x05, 0x06, 0x07 }));
        var participant = new Participant(
            Guid.NewGuid(),
            "Participant-2",
            publicKey,
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var epoch = new Epoch(Guid.NewGuid(), 2, DateTime.UtcNow);

        var epochMember = new EpochMember(
            Guid.NewGuid(),
            epoch.Id,
            participant.Id,
            DateTime.UtcNow);

        var service = new RevocationPolicyService();

        Assert.Throws<InvalidOperationException>(() =>
            service.RevokeParticipant(
                epoch,
                participant,
                new[] { epochMember },
                "Attempt to revoke in draft epoch",
                DateTime.UtcNow));
    }
}