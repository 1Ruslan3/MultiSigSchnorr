using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Hashing;

public sealed class HashToScalarService : IHashToScalarService
{
    private readonly ICurveContext _curveContext;
    private readonly Sha256HashService _hashService;

    public HashToScalarService(ICurveContext curveContext, Sha256HashService hashService)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
    }

    public ScalarValue HashToScalar(string domainTag, params byte[][] parts)
    {
        var hash = _hashService.ComputeHash(domainTag, parts);
        return _curveContext.ReduceScalar(hash);
    }
}