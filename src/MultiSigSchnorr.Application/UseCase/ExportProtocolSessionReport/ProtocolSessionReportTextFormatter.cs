using System.Text;

namespace MultiSigSchnorr.Application.UseCases.ExportProtocolSessionReport;

public sealed class ProtocolSessionReportTextFormatter
{
    public string Format(ProtocolSessionReportDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new StringBuilder();

        sb.AppendLine("MULTISIG SCHNORR PROTOCOL SESSION REPORT");
        sb.AppendLine(new string('=', 80));
        sb.AppendLine();

        sb.AppendLine("GENERAL");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"Session ID: {report.SessionId}");
        sb.AppendLine($"Epoch ID: {report.EpochId}");
        sb.AppendLine($"Epoch Number: {report.EpochNumber}");
        sb.AppendLine($"Session Status: {report.SessionStatus}");
        sb.AppendLine($"Created UTC: {report.CreatedUtc:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Completed UTC: {(report.CompletedUtc.HasValue ? report.CompletedUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "—")}");
        sb.AppendLine();

        sb.AppendLine("PROTOCOL FLAGS");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"All Commitments Published: {report.AllCommitmentsPublished}");
        sb.AppendLine($"All Nonces Revealed: {report.AllNoncesRevealed}");
        sb.AppendLine($"All Partial Signatures Submitted: {report.AllPartialSignaturesSubmitted}");
        sb.AppendLine();

        sb.AppendLine("CRYPTO PARAMETERS");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"Message Digest Hex: {report.MessageDigestHex}");
        sb.AppendLine($"Aggregate Public Key Hex: {report.AggregatePublicKeyHex}");
        sb.AppendLine($"Aggregate Nonce Point Hex: {report.AggregateNoncePointHex ?? "—"}");
        sb.AppendLine($"Challenge Hex: {report.ChallengeHex ?? "—"}");
        sb.AppendLine($"Aggregate Signature Nonce Point Hex: {report.AggregateSignatureNoncePointHex ?? "—"}");
        sb.AppendLine($"Aggregate Signature Scalar Hex: {report.AggregateSignatureScalarHex ?? "—"}");
        sb.AppendLine();

        sb.AppendLine("PARTICIPANTS");
        sb.AppendLine(new string('-', 80));

        foreach (var participant in report.Participants)
        {
            sb.AppendLine($"Participant ID: {participant.ParticipantId}");
            sb.AppendLine($"Display Name: {participant.DisplayName}");
            sb.AppendLine($"Public Key Hex: {participant.PublicKeyHex}");
            sb.AppendLine($"Aggregation Coefficient Hex: {participant.AggregationCoefficientHex}");
            sb.AppendLine($"Has Commitment: {participant.HasCommitment}");
            sb.AppendLine($"Has Reveal: {participant.HasReveal}");
            sb.AppendLine($"Has Partial Signature: {participant.HasPartialSignature}");
            sb.AppendLine($"Commitment Hex: {participant.CommitmentHex ?? "—"}");
            sb.AppendLine($"Public Nonce Point Hex: {participant.PublicNoncePointHex ?? "—"}");
            sb.AppendLine($"Partial Signature Hex: {participant.PartialSignatureHex ?? "—"}");
            sb.AppendLine(new string('-', 80));
        }

        return sb.ToString();
    }
}