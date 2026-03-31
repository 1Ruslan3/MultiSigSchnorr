using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace MultiSigSchnorr.Crypto.Hashing;

public sealed class Sha256HashService
{
    public byte[] ComputeHash(string domainTag, params byte[][] parts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainTag);

        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        AppendPart(hasher, Encoding.UTF8.GetBytes(domainTag));

        foreach (var part in parts)
        {
            ArgumentNullException.ThrowIfNull(part);
            AppendPart(hasher, part);
        }

        return hasher.GetHashAndReset();
    }

    private static void AppendPart(IncrementalHash hasher, ReadOnlySpan<byte> data)
    {
        Span<byte> lengthPrefix = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthPrefix, data.Length);

        hasher.AppendData(lengthPrefix);
        hasher.AppendData(data);
    }
}