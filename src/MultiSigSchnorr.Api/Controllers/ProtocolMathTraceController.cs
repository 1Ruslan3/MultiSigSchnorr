using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MultiSigSchnorr.Contracts.ProtocolMath;
using Org.BouncyCastle.Asn1.Sec;

using BcBigInteger = Org.BouncyCastle.Math.BigInteger;
using BcECCurve = Org.BouncyCastle.Math.EC.ECCurve;
using BcECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace MultiSigSchnorr.Api.Controllers;

[ApiController]
[Route("api/protocol-sessions")]
public sealed class ProtocolMathTraceController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    private static readonly Lazy<CurveRuntime> Curve = new(CreateCurveRuntime);

    public ProtocolMathTraceController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    [HttpGet("{sessionId:guid}/math-trace")]
    public async Task<ActionResult<ProtocolMathTraceApiResponse>> GetMathTrace(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var sessionState = await LoadSessionStateAsync(sessionId, cancellationToken);

        if (sessionState is null)
            return NotFound(new { error = $"Protocol session '{sessionId}' was not found." });

        var trace = BuildTrace(sessionState);

        return Ok(trace);
    }

    private async Task<object?> LoadSessionStateAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var handlerType = Type.GetType(
            "MultiSigSchnorr.Application.UseCases.GetSessionState.GetSessionStateHandler, MultiSigSchnorr.Application",
            throwOnError: false);

        var requestType = Type.GetType(
            "MultiSigSchnorr.Application.UseCases.GetSessionState.GetSessionStateRequest, MultiSigSchnorr.Application",
            throwOnError: false);

        if (handlerType is null || requestType is null)
            throw new InvalidOperationException("GetSessionState use case types were not found.");

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var request = CreateRequest(requestType, sessionId);

        var method = handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.Name == "HandleAsync")
            .FirstOrDefault(x =>
            {
                var parameters = x.GetParameters();

                return parameters.Length >= 1 && parameters[0].ParameterType == requestType;
            });

        if (method is null)
            throw new InvalidOperationException("GetSessionStateHandler.HandleAsync method was not found.");

        var parameters = method.GetParameters().Length == 1
            ? new[] { request }
            : new[] { request, cancellationToken };

        var result = method.Invoke(handler, parameters);

        if (result is null)
            return null;

        if (result is Task task)
        {
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty(
                "Result",
                BindingFlags.Public | BindingFlags.Instance);

            return resultProperty?.GetValue(task);
        }

        return result;
    }

    private static object CreateRequest(Type requestType, Guid sessionId)
    {
        var guidConstructor = requestType.GetConstructor(new[] { typeof(Guid) });
        if (guidConstructor is not null)
            return guidConstructor.Invoke(new object[] { sessionId });

        var request = Activator.CreateInstance(requestType)
            ?? throw new InvalidOperationException("GetSessionStateRequest could not be created.");

        var sessionIdProperty = requestType.GetProperty(
            "SessionId",
            BindingFlags.Public | BindingFlags.Instance);

        if (sessionIdProperty is null || !sessionIdProperty.CanWrite)
            throw new InvalidOperationException("GetSessionStateRequest.SessionId settable property was not found.");

        sessionIdProperty.SetValue(request, sessionId);

        return request;
    }

    private static ProtocolMathTraceApiResponse BuildTrace(object session)
    {
        var challengeHex = GetString(session, "ChallengeHex");

        var participants = GetEnumerable(session, "Participants")
            .Select(x => BuildParticipantTrace(x, challengeHex))
            .ToList();

        var trace = new ProtocolMathTraceApiResponse
        {
            SessionId = GetValue<Guid>(session, "SessionId"),
            EpochId = GetValue<Guid>(session, "EpochId"),
            EpochNumber = GetValue<int>(session, "EpochNumber"),
            SessionStatus = GetString(session, "SessionStatus"),
            ProtectionMode = GetString(session, "ProtectionMode"),
            MessageDigestHex = GetString(session, "MessageDigestHex"),
            AggregatePublicKeyHex = GetString(session, "AggregatePublicKeyHex"),
            AggregateNoncePointHex = FirstNonEmpty(
                GetString(session, "AggregateNoncePointHex"),
                GetString(session, "AggregateSignatureNoncePointHex")),
            ChallengeHex = challengeHex,
            AggregateSignatureScalarHex = FirstNonEmpty(
                GetString(session, "AggregateSignatureScalarHex"),
                GetString(session, "SignatureScalarHex")),
            Participants = participants
        };

        var finalVerification = ComputeFinalVerification(trace);

        return new ProtocolMathTraceApiResponse
        {
            SessionId = trace.SessionId,
            EpochId = trace.EpochId,
            EpochNumber = trace.EpochNumber,
            SessionStatus = trace.SessionStatus,
            ProtectionMode = trace.ProtectionMode,
            MessageDigestHex = trace.MessageDigestHex,
            AggregatePublicKeyHex = trace.AggregatePublicKeyHex,
            AggregateNoncePointHex = trace.AggregateNoncePointHex,
            ChallengeHex = trace.ChallengeHex,
            AggregateSignatureScalarHex = trace.AggregateSignatureScalarHex,
            Participants = participants,
            FinalVerification = finalVerification,
            Steps = BuildSteps(trace, participants, finalVerification)
        };
    }

    private static ProtocolMathParticipantTraceApiResponse BuildParticipantTrace(
        object participant,
        string challengeHex)
    {
        var publicKeyHex = GetString(participant, "PublicKeyHex");
        var aggregationCoefficientHex = GetString(participant, "AggregationCoefficientHex");
        var commitmentHex = GetString(participant, "CommitmentHex");
        var publicNoncePointHex = GetString(participant, "PublicNoncePointHex");
        var partialSignatureHex = GetString(participant, "PartialSignatureHex");

        var recomputedCommitment = ComputeCommitment(publicNoncePointHex);

        bool? commitmentMatches = null;
        if (!string.IsNullOrWhiteSpace(commitmentHex) &&
            !string.IsNullOrWhiteSpace(recomputedCommitment))
        {
            commitmentMatches = string.Equals(
                commitmentHex,
                recomputedCommitment,
                StringComparison.OrdinalIgnoreCase);
        }

        var partialCheck = ComputePartialSignatureCheck(
            partialSignatureHex,
            publicNoncePointHex,
            aggregationCoefficientHex,
            publicKeyHex,
            challengeHex);

        return new ProtocolMathParticipantTraceApiResponse
        {
            ParticipantId = GetValue<Guid>(participant, "ParticipantId"),
            DisplayName = GetString(participant, "DisplayName"),
            PublicKeyHex = publicKeyHex,
            AggregationCoefficientHex = aggregationCoefficientHex,
            CommitmentHex = commitmentHex,
            PublicNoncePointHex = publicNoncePointHex,
            PartialSignatureHex = partialSignatureHex,
            CommitmentRecomputedHex = recomputedCommitment,
            CommitmentMatchesPublicNonce = commitmentMatches,
            PartialSignatureLeftPointHex = partialCheck.LeftPointHex,
            PartialSignatureRightPointHex = partialCheck.RightPointHex,
            PartialSignatureEquationHolds = partialCheck.EquationHolds
        };
    }

    private static IReadOnlyList<ProtocolMathStepApiResponse> BuildSteps(
        ProtocolMathTraceApiResponse trace,
        IReadOnlyList<ProtocolMathParticipantTraceApiResponse> participants,
        ProtocolMathFinalVerificationApiResponse finalVerification)
    {
        return new List<ProtocolMathStepApiResponse>
        {
            new()
            {
                Number = 1,
                Stage = "Message Digest",
                Title = "Хеширование сообщения",
                Formula = "e = H(m)",
                Description = "Сообщение преобразуется в digest фиксированной длины. Именно digest участвует в вычислении challenge.",
                IsComplete = HasValue(trace.MessageDigestHex),
                Values = new Dictionary<string, string>
                {
                    ["e"] = trace.MessageDigestHex
                }
            },
            new()
            {
                Number = 2,
                Stage = "Key Aggregation",
                Title = "Агрегация открытых ключей",
                Formula = "L = H(P₁ || ... || Pₙ),  aᵢ = H(L || Pᵢ),  X = Σ(aᵢ · Pᵢ)",
                Description = "Каждый открытый ключ получает коэффициент агрегации. Итоговый ключ X связывает подпись со всем набором подписантов.",
                IsComplete = HasValue(trace.AggregatePublicKeyHex),
                Values = BuildKeyAggregationValues(trace, participants)
            },
            new()
            {
                Number = 3,
                Stage = "Commitment Verification",
                Title = "Проверка commitments",
                Formula = "Cᵢ ?= H(Rᵢ)",
                Description = "После раскрытия public nonce можно пересчитать commitment и сравнить его с ранее опубликованным значением.",
                IsComplete = participants.Count > 0 &&
                             participants.All(x => x.CommitmentMatchesPublicNonce == true),
                Values = BuildCommitmentValues(participants)
            },
            new()
            {
                Number = 4,
                Stage = "Nonce Aggregation",
                Title = "Агрегация public nonce",
                Formula = "R = R₁ + R₂ + ... + Rₙ",
                Description = "Публичные nonce всех участников складываются в aggregate nonce point R.",
                IsComplete = HasValue(trace.AggregateNoncePointHex),
                Values = BuildNonceValues(trace, participants)
            },
            new()
            {
                Number = 5,
                Stage = "Challenge",
                Title = "Вычисление challenge",
                Formula = "c = H(X || R || e)",
                Description = "Challenge связывает сообщение, агрегированный открытый ключ и агрегированный nonce.",
                IsComplete = HasValue(trace.ChallengeHex),
                Values = new Dictionary<string, string>
                {
                    ["X"] = trace.AggregatePublicKeyHex,
                    ["R"] = trace.AggregateNoncePointHex,
                    ["e"] = trace.MessageDigestHex,
                    ["c"] = trace.ChallengeHex
                }
            },
            new()
            {
                Number = 6,
                Stage = "Partial Signature Checks",
                Title = "Проверка частичных подписей",
                Formula = "sᵢ · G ?= Rᵢ + c · aᵢ · Pᵢ",
                Description = "Для каждой частичной подписи вычисляются левая и правая части публичного равенства. Секретные xᵢ и rᵢ не раскрываются.",
                IsComplete = participants.Count > 0 &&
                             participants.All(x => x.PartialSignatureEquationHolds == true),
                Values = BuildPartialSignatureValues(participants)
            },
            new()
            {
                Number = 7,
                Stage = "Aggregate Signature",
                Title = "Агрегация частичных подписей",
                Formula = "s = s₁ + s₂ + ... + sₙ mod q",
                Description = "Итоговый скаляр подписи является суммой частичных подписей по модулю порядка группы.",
                IsComplete = HasValue(trace.AggregateSignatureScalarHex),
                Values = new Dictionary<string, string>
                {
                    ["s"] = trace.AggregateSignatureScalarHex
                }
            },
            new()
            {
                Number = 8,
                Stage = "Final Verification",
                Title = "Проверка агрегированной подписи",
                Formula = "s · G ?= R + c · X",
                Description = "Финальное равенство подтверждает корректность агрегированной подписи относительно сообщения, aggregate public key и aggregate nonce.",
                IsComplete = finalVerification.EquationHolds == true,
                Values = new Dictionary<string, string>
                {
                    ["left = s · G"] = finalVerification.LeftPointHex,
                    ["right = R + c · X"] = finalVerification.RightPointHex,
                    ["result"] = finalVerification.EquationHolds?.ToString() ?? "not available"
                }
            }
        };
    }

    private static Dictionary<string, string> BuildKeyAggregationValues(
        ProtocolMathTraceApiResponse trace,
        IReadOnlyList<ProtocolMathParticipantTraceApiResponse> participants)
    {
        var values = new Dictionary<string, string>
        {
            ["X"] = trace.AggregatePublicKeyHex
        };

        foreach (var participant in participants)
        {
            values[$"{participant.DisplayName}: Pᵢ"] = participant.PublicKeyHex;
            values[$"{participant.DisplayName}: aᵢ"] = participant.AggregationCoefficientHex;
        }

        return values;
    }

    private static Dictionary<string, string> BuildCommitmentValues(
        IReadOnlyList<ProtocolMathParticipantTraceApiResponse> participants)
    {
        var values = new Dictionary<string, string>();

        foreach (var participant in participants)
        {
            values[$"{participant.DisplayName}: Cᵢ stored"] = participant.CommitmentHex;
            values[$"{participant.DisplayName}: H(Rᵢ) recomputed"] = participant.CommitmentRecomputedHex;
            values[$"{participant.DisplayName}: match"] = participant.CommitmentMatchesPublicNonce?.ToString() ?? "not available";
        }

        return values;
    }

    private static Dictionary<string, string> BuildNonceValues(
        ProtocolMathTraceApiResponse trace,
        IReadOnlyList<ProtocolMathParticipantTraceApiResponse> participants)
    {
        var values = new Dictionary<string, string>
        {
            ["R"] = trace.AggregateNoncePointHex
        };

        foreach (var participant in participants)
        {
            values[$"{participant.DisplayName}: Rᵢ"] = participant.PublicNoncePointHex;
        }

        return values;
    }

    private static Dictionary<string, string> BuildPartialSignatureValues(
        IReadOnlyList<ProtocolMathParticipantTraceApiResponse> participants)
    {
        var values = new Dictionary<string, string>();

        foreach (var participant in participants)
        {
            values[$"{participant.DisplayName}: left"] = participant.PartialSignatureLeftPointHex;
            values[$"{participant.DisplayName}: right"] = participant.PartialSignatureRightPointHex;
            values[$"{participant.DisplayName}: result"] = participant.PartialSignatureEquationHolds?.ToString() ?? "not available";
        }

        return values;
    }

    private static string ComputeCommitment(string publicNoncePointHex)
    {
        if (string.IsNullOrWhiteSpace(publicNoncePointHex))
            return string.Empty;

        try
        {
            var bytes = Convert.FromHexString(publicNoncePointHex);
            return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
        }
        catch
        {
            return string.Empty;
        }
    }

    private static ProtocolMathPointCheck ComputePartialSignatureCheck(
        string partialSignatureHex,
        string publicNoncePointHex,
        string aggregationCoefficientHex,
        string publicKeyHex,
        string challengeHex)
    {
        if (!HasValue(partialSignatureHex) ||
            !HasValue(publicNoncePointHex) ||
            !HasValue(aggregationCoefficientHex) ||
            !HasValue(publicKeyHex) ||
            !HasValue(challengeHex))
        {
            return new ProtocolMathPointCheck();
        }

        try
        {
            var runtime = Curve.Value;

            var s = ToScalar(partialSignatureHex);
            var c = ToScalar(challengeHex);
            var a = ToScalar(aggregationCoefficientHex);

            var publicNonce = runtime.Curve.DecodePoint(Convert.FromHexString(publicNoncePointHex));
            var publicKey = runtime.Curve.DecodePoint(Convert.FromHexString(publicKeyHex));

            var left = runtime.G.Multiply(s).Normalize();
            var coefficient = c.Multiply(a).Mod(runtime.N);
            var right = publicNonce.Add(publicKey.Multiply(coefficient)).Normalize();

            return new ProtocolMathPointCheck
            {
                LeftPointHex = ToPointHex(left),
                RightPointHex = ToPointHex(right),
                EquationHolds = left.Equals(right)
            };
        }
        catch
        {
            return new ProtocolMathPointCheck();
        }
    }

    private static ProtocolMathFinalVerificationApiResponse ComputeFinalVerification(
        ProtocolMathTraceApiResponse trace)
    {
        if (!HasValue(trace.AggregateSignatureScalarHex) ||
            !HasValue(trace.AggregateNoncePointHex) ||
            !HasValue(trace.ChallengeHex) ||
            !HasValue(trace.AggregatePublicKeyHex))
        {
            return new ProtocolMathFinalVerificationApiResponse();
        }

        try
        {
            var runtime = Curve.Value;

            var s = ToScalar(trace.AggregateSignatureScalarHex);
            var c = ToScalar(trace.ChallengeHex);

            var aggregateNonce = runtime.Curve.DecodePoint(Convert.FromHexString(trace.AggregateNoncePointHex));
            var aggregatePublicKey = runtime.Curve.DecodePoint(Convert.FromHexString(trace.AggregatePublicKeyHex));

            var left = runtime.G.Multiply(s).Normalize();
            var right = aggregateNonce.Add(aggregatePublicKey.Multiply(c)).Normalize();

            return new ProtocolMathFinalVerificationApiResponse
            {
                LeftPointHex = ToPointHex(left),
                RightPointHex = ToPointHex(right),
                EquationHolds = left.Equals(right)
            };
        }
        catch
        {
            return new ProtocolMathFinalVerificationApiResponse();
        }
    }

    private static CurveRuntime CreateCurveRuntime()
    {
        var parameters = SecNamedCurves.GetByName("secp256r1");

        return new CurveRuntime(
            parameters.Curve,
            parameters.G,
            parameters.N);
    }

    private static BcBigInteger ToScalar(string hex)
    {
        return new BcBigInteger(1, Convert.FromHexString(hex)).Mod(Curve.Value.N);
    }

    private static string ToPointHex(BcECPoint point)
    {
        return Convert.ToHexString(point.Normalize().GetEncoded(false));
    }

    private static IReadOnlyList<object> GetEnumerable(object source, string propertyName)
    {
        var value = source.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(source);

        if (value is null)
            return Array.Empty<object>();

        if (value is IEnumerable enumerable)
            return enumerable.Cast<object>().ToList();

        return Array.Empty<object>();
    }

    private static T GetValue<T>(object source, string propertyName)
    {
        var value = source.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(source);

        if (value is null)
            return default!;

        if (value is T typed)
            return typed;

        return (T)Convert.ChangeType(value, typeof(T));
    }

    private static string GetString(object source, string propertyName)
    {
        var value = source.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(source);

        return value?.ToString() ?? string.Empty;
    }

    private static bool HasValue(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
    }

    private sealed record CurveRuntime(
        BcECCurve Curve,
        BcECPoint G,
        BcBigInteger N);

    private sealed class ProtocolMathPointCheck
    {
        public string LeftPointHex { get; init; } = string.Empty;
        public string RightPointHex { get; init; } = string.Empty;
        public bool? EquationHolds { get; init; }
    }
}
