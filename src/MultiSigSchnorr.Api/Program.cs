using Microsoft.EntityFrameworkCore;
using MultiSigSchnorr.Api.Development;
using MultiSigSchnorr.Application.Audit;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Application.UseCases.CreateProtocolSession;
using MultiSigSchnorr.Application.UseCases.ExportProtocolSessionReport;
using MultiSigSchnorr.Application.UseCases.GetAuditLog;
using MultiSigSchnorr.Application.UseCases.GetEpochAdministrationState;
using MultiSigSchnorr.Application.UseCases.GetProtocolSessionHistory;
using MultiSigSchnorr.Application.UseCases.GetSessionState;
using MultiSigSchnorr.Application.UseCases.PublishCommitment;
using MultiSigSchnorr.Application.UseCases.RevealNonce;
using MultiSigSchnorr.Application.UseCases.RevokeParticipantInActiveEpoch;
using MultiSigSchnorr.Application.UseCases.SubmitPartialSignature;
using MultiSigSchnorr.Application.UseCases.TransitionToNextEpoch;
using MultiSigSchnorr.Application.UseCases.VerifyProtocolSessionSignature;
using MultiSigSchnorr.Crypto.Aggregation;
using MultiSigSchnorr.Crypto.Commitments;
using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Crypto.Hashing;
using MultiSigSchnorr.Crypto.Nonces;
using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Infrastructure.Persistence;
using MultiSigSchnorr.Infrastructure.Persistence.Repositories;
using MultiSigSchnorr.Infrastructure.Repositories;
using MultiSigSchnorr.Protocol.Epochs;
using MultiSigSchnorr.Protocol.Revocation;
using MultiSigSchnorr.Protocol.Sessions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("MultiSigSchnorrDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'MultiSigSchnorrDb' is not configured.");
}

builder.Services.AddDbContext<MultiSigSchnorrDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddSingleton<P256CurveContext>();
builder.Services.AddSingleton<MessageDigestService>();
builder.Services.AddSingleton<Sha256HashService>();
builder.Services.AddSingleton<SystemRandomSource>();
builder.Services.AddSingleton<EpochParticipationGuard>();
builder.Services.AddSingleton<EpochTransitionService>();
builder.Services.AddSingleton<RevocationPolicyService>();

builder.Services.AddSingleton<PublicKeyGenerationService>(sp =>
    new PublicKeyGenerationService(sp.GetRequiredService<P256CurveContext>()));

builder.Services.AddSingleton<HashToScalarService>(sp =>
    new HashToScalarService(
        sp.GetRequiredService<P256CurveContext>(),
        sp.GetRequiredService<Sha256HashService>()));

builder.Services.AddSingleton<ChallengeService>(sp =>
    new ChallengeService(
        sp.GetRequiredService<HashToScalarService>()));

builder.Services.AddSingleton<CommitmentService>(sp =>
    new CommitmentService(
        sp.GetRequiredService<Sha256HashService>()));

builder.Services.AddSingleton<SecureNonceGenerator>(sp =>
    new SecureNonceGenerator(
        sp.GetRequiredService<P256CurveContext>(),
        sp.GetRequiredService<SystemRandomSource>()));

builder.Services.AddSingleton<PartialSignatureService>(sp =>
    new PartialSignatureService(
        sp.GetRequiredService<P256CurveContext>()));

builder.Services.AddSingleton<AggregateSignatureVerifier>(sp =>
    new AggregateSignatureVerifier(
        sp.GetRequiredService<P256CurveContext>(),
        sp.GetRequiredService<ChallengeService>()));

builder.Services.AddSingleton<AggregateKeyService>(sp =>
    new AggregateKeyService(
        sp.GetRequiredService<P256CurveContext>(),
        sp.GetRequiredService<HashToScalarService>()));

builder.Services.AddSingleton<NPartyCommitmentProtocolService>(sp =>
    new NPartyCommitmentProtocolService(
        sp.GetRequiredService<PublicKeyGenerationService>(),
        sp.GetRequiredService<AggregateKeyService>(),
        sp.GetRequiredService<SecureNonceGenerator>(),
        sp.GetRequiredService<CommitmentService>(),
        sp.GetRequiredService<ChallengeService>(),
        sp.GetRequiredService<PartialSignatureService>(),
        sp.GetRequiredService<AggregateSignatureVerifier>(),
        sp.GetRequiredService<P256CurveContext>(),
        sp.GetRequiredService<EpochParticipationGuard>()));

builder.Services.AddScoped<IEpochRepository, PostgresEpochRepository>();
builder.Services.AddScoped<IParticipantRepository, PostgresParticipantRepository>();
builder.Services.AddScoped<IEpochMemberRepository, PostgresEpochMemberRepository>();
builder.Services.AddScoped<IAuditLogRepository, PostgresAuditLogRepository>();
builder.Services.AddScoped<IProtocolSessionProjectionRepository, PostgresProtocolSessionProjectionRepository>();

builder.Services.AddSingleton<ISignatureSessionRepository, InMemorySignatureSessionRepository>();
builder.Services.AddSingleton<IProtocolSessionRepository, InMemoryProtocolSessionRepository>();
builder.Services.AddSingleton<IPrivateKeyMaterialRepository, InMemoryPrivateKeyMaterialRepository>();

builder.Services.AddScoped<DevelopmentDataSeeder>();
builder.Services.AddSingleton<ProtocolSessionReportTextFormatter>();
builder.Services.AddScoped<AuditLogService>();

builder.Services.AddScoped<CreateProtocolSessionHandler>();
builder.Services.AddScoped<PublishCommitmentHandler>();
builder.Services.AddScoped<RevealNonceHandler>();
builder.Services.AddScoped<SubmitPartialSignatureHandler>();
builder.Services.AddScoped<GetSessionStateHandler>();
builder.Services.AddScoped<VerifyProtocolSessionSignatureHandler>();
builder.Services.AddScoped<GetProtocolSessionHistoryHandler>();
builder.Services.AddScoped<ExportProtocolSessionReportHandler>();
builder.Services.AddScoped<GetEpochAdministrationStateHandler>();
builder.Services.AddScoped<RevokeParticipantInActiveEpochHandler>();
builder.Services.AddScoped<TransitionToNextEpochHandler>();
builder.Services.AddScoped<GetAuditLogHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MultiSigSchnorrDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
    await seeder.SeedAsync();
}

app.MapGet("/", async (DevelopmentDataSeeder dataSeeder, CancellationToken cancellationToken) =>
{
    await dataSeeder.SeedAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "MultiSigSchnorr.Api",
        status = "running",
        openApi = "/openapi/v1.json",
        protocolSessions = "/api/protocol-sessions",
        admin = "/api/admin/epoch-management",
        audit = "/api/audit",
        storage = "PostgreSQL for AuditLog, Epoch, Participant and EpochMember; in-memory for protocol sessions and private key material",
        seeded = dataSeeder.Snapshot
    });
});

// Пока без https, чтобы не ловить бессмысленные предупреждения локально
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program
{
}