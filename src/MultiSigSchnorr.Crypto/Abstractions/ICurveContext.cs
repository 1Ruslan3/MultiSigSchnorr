using System.Numerics;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface ICurveContext
{
    string CurveName { get; }
    int ScalarSizeBytes { get; }
    BigInteger Order { get; }

    bool IsValidPoint(PointValue point);
    PointValue MultiplyBasePoint(ScalarValue scalar);
    PointValue MultiplyPoint(PointValue point, ScalarValue scalar);
    PointValue AddPoints(PointValue left, PointValue right);
    ScalarValue ReduceScalar(ReadOnlySpan<byte> data);
}