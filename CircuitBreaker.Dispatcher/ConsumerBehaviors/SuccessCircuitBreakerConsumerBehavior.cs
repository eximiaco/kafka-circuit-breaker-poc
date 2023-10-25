using CircuitBreaker.Dispatcher.Extensions;
using CircuitBreaker.Dispatcher.Policies;
using Silverback.Messaging.Broker.Behaviors;

namespace CircuitBreaker.Dispatcher.ConsumerBehaviors;

public class SuccessCircuitBreakerConsumerBehavior : IConsumerBehavior
{
    public int SortIndex => 2001;

    public async Task HandleAsync(ConsumerPipelineContext context, ConsumerBehaviorHandler next)
    {
        await next(context);

        var circuitId = context.GetCircuitBreakerId();

        if (!string.IsNullOrEmpty(circuitId))
        {
            var circuitState = context.GetCircuitBreakerState();
            if (circuitState == ECircuitState.Open || circuitState == ECircuitState.HalfOpen)
            {
                var circuitRepository = context.ServiceProvider.GetRequiredService<ICircuitRepository>();

                var circuit = await circuitRepository.GetOrAddByIdAsync(circuitId);
                if (circuit.IsOpen is true)
                    await circuit.AttemptAsync();
                else if (circuit.IsHalfOpen is true)
                    await circuit.CloseAsync();
            }
        }
    }
}
