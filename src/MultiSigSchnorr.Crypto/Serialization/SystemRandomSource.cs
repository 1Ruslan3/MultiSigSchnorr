using System.Security.Cryptography;
using MultiSigSchnorr.Domain.Abstractions;

namespace MultiSigSchnorr.Crypto.Security;

public sealed class SystemRandomSource : IRandomSource
{
    public void Fill(Span<byte> buffer)
    {
        RandomNumberGenerator.Fill(buffer);
    }
}