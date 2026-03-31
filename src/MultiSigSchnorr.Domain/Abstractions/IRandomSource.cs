namespace MultiSigSchnorr.Domain.Abstractions;

public interface IRandomSource
{
    void Fill(Span<byte> buffer);
}
