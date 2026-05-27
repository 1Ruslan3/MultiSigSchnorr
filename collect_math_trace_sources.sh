OUTPUT_FILE="math_trace_sources_report.txt"

{

echo "MultiSigSchnorr math trace source report"

echo "Generated at: $(date)"

echo "Project root: $(pwd)"

echo "============================================================"

echo ""

echo "============================================================"

echo "CommitmentService"

echo "============================================================"

sed -n '1,220p' src/MultiSigSchnorr.Crypto/Commitments/CommitmentService.cs

echo ""

echo "============================================================"

echo "Sha256HashService"

echo "============================================================"

sed -n '1,220p' src/MultiSigSchnorr.Crypto/Hashing/Sha256HashService.cs

echo ""

echo "============================================================"

echo "ChallengeService"

echo "============================================================"

sed -n '1,220p' src/MultiSigSchnorr.Crypto/Schnorr/ChallengeService.cs

echo ""

echo "============================================================"

echo "AggregateSignatureVerifier"

echo "============================================================"

sed -n '1,220p' src/MultiSigSchnorr.Crypto/Schnorr/AggregateSignatureVerifier.cs

echo ""

echo "============================================================"

echo "PartialSignatureService"

echo "============================================================"

sed -n '1,220p' src/MultiSigSchnorr.Crypto/Schnorr/PartialSignatureService.cs

echo ""

echo "============================================================"

echo "PointValue"

echo "============================================================"

sed -n '1,180p' src/MultiSigSchnorr.Domain/ValueObjects/PointValue.cs

echo ""

echo "============================================================"

echo "ScalarValue"

echo "============================================================"

sed -n '1,180p' src/MultiSigSchnorr.Domain/ValueObjects/ScalarValue.cs

echo ""

echo "============================================================"

echo "P256CurveContext"

echo "============================================================"

sed -n '1,180p' src/MultiSigSchnorr.Crypto/Curves/P256CurveContext.cs

echo ""

echo "============================================================"

echo "HashToScalarService"

echo "============================================================"

sed -n '1,220p' src/MultiSigSchnorr.Crypto/Hashing/HashToScalarService.cs

echo ""

echo "============================================================"

echo "AggregateKeyService"

echo "============================================================"

sed -n '1,220p' src/MultiSigSchnorr.Crypto/Aggregation/AggregateKeyService.cs

echo ""

echo "============================================================"

echo "NPartyCommitmentProtocolService"

echo "============================================================"

sed -n '1,340p' src/MultiSigSchnorr.Protocol/Sessions/NPartyCommitmentProtocolService.cs

echo ""

echo "============================================================"

echo "DomainSeparationTags"

echo "============================================================"

sed -n '1,120p' src/MultiSigSchnorr.Domain/Constants/DomainSeparationTags.cs

echo ""

echo "============================================================"

echo "Search: commitment / challenge / signature / verifier"

echo "============================================================"

grep -R "CreateCommitment\|VerifyCommitment\|ComputeChallenge\|CreatePartialSignature\|AggregateSignatureVerifier\|HashToScalar\|DomainSeparation" src/MultiSigSchnorr.Crypto src/MultiSigSchnorr.Protocol src/MultiSigSchnorr.Domain -n

echo ""

echo "============================================================"

echo "Search: point/scalar operations"

echo "============================================================"

grep -R "MultiplyBasePoint\|MultiplyPoint\|AddPoints\|SubtractMod\|AddMod\|MultiplyMod\|ToHex\|FromHex" src/MultiSigSchnorr.Crypto src/MultiSigSchnorr.Domain -n

echo ""

echo "============================================================"

echo "Done."

} > "$OUTPUT_FILE" 2>&1

echo "Готово. Отчёт создан: $OUTPUT_FILE"

