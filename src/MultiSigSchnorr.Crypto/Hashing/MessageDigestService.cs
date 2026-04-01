using System.Security.Cryptography;
using System.Text;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Hashing;

public sealed class MessageDigestService
{
    public MessageDigestValue DigestUtf8(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var bytes = Encoding.UTF8.GetBytes(message);
        var hash = SHA256.HashData(bytes);

        return new MessageDigestValue(hash);
    }

    public MessageDigestValue DigestBytes(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Length == 0)
            throw new ArgumentException("Message bytes cannot be empty.", nameof(message));

        var hash = SHA256.HashData(message);
        return new MessageDigestValue(hash);
    }
}