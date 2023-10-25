using CircuitBreaker.Dispatcher.Extensions;
using CircuitBreaker.Dispatcher.Interfaces;
using Silverback.Messaging.Broker.Behaviors;

namespace CircuitBreaker.Dispatcher.ConsumerBehaviors;

public class AddCircuitBreakerHeaderConsumerBehavior : IConsumerBehavior
{
    public int SortIndex => 98;

    public async Task HandleAsync(ConsumerPipelineContext context, ConsumerBehaviorHandler next)
    {
        var circuitId = context.GetCircuitBreakerId();

        if (string.IsNullOrEmpty(circuitId))
        {
            var circuitIdProvider = context.ServiceProvider.GetRequiredService<ICircuitBreakerIdProvider>();

            circuitId = circuitIdProvider.GetId(context.Consumer.Endpoint.Name);

            context.SetCircuitBreakerId(circuitId);
        }

        await next(context);
    }
}
