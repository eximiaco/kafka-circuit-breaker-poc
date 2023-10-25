using CircuitBreaker.Dispatcher.Policies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Silverback.Messaging.Broker.Behaviors;
using Silverback.Messaging.Messages;

namespace CircuitBreaker.Dispatcher.Extensions;

public static class ConsumerPipelineContextExtensions
{
    private const string CIRCUIT_ID_HEADER = "x-circuit-breaker-id";
    private const string CIRCUIT_STATE_HEADER = "x-circuit-breaker-state";

    public static string? GetCircuitBreakerId(this ConsumerPipelineContext context)
        => context.Envelope.Headers.GetValueOrDefault(CIRCUIT_ID_HEADER, typeof(string)) as string;

    public static ECircuitState? GetCircuitBreakerState(this ConsumerPipelineContext context)
       => context.Envelope.Headers.Contains(CIRCUIT_STATE_HEADER) ? context.Envelope.Headers.GetValueOrDefault<ECircuitState>(CIRCUIT_STATE_HEADER) : null;

    public static void SetCircuitBreakerId(this ConsumerPipelineContext context, string? circuitId)
    {
        if (string.IsNullOrEmpty(circuitId))
            context.Envelope.Headers.Remove(CIRCUIT_ID_HEADER);
        else
            context.Envelope.Headers[CIRCUIT_ID_HEADER] = circuitId;
    }

    public static void SetCircuitBreakerState(this ConsumerPipelineContext context, ECircuitState? state)
    {
        if (state.HasValue)
            context.Envelope.Headers[CIRCUIT_STATE_HEADER] = state.Value.ToString();
        else
            context.Envelope.Headers.Remove(CIRCUIT_STATE_HEADER);
        
    }
}
