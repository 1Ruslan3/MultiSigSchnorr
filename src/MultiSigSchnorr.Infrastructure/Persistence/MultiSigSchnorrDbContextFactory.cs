using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MultiSigSchnorr.Infrastructure.Persistence;

public sealed class MultiSigSchnorrDbContextFactory
    : IDesignTimeDbContextFactory<MultiSigSchnorrDbContext>
{
    public MultiSigSchnorrDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MultiSigSchnorrDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5433;Database=multisig_schnorr;Username=multisig_user;Password=multisig_password");

        return new MultiSigSchnorrDbContext(optionsBuilder.Options);
    }
}