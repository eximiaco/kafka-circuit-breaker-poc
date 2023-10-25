using CircuitBreaker.Dispatcher.Extensions;
using CircuitBreaker.Dispatcher.Policies;
using Silverback.Messaging.Broker.Behaviors;

namespace CircuitBreaker.Dispatcher.ConsumerBehaviors;

public class CheckCircuitBreakerConsumerBehavior : IConsumerBehavior
{
    public int SortIndex => 99;

    public async Task HandleAsync(ConsumerPipelineContext context, ConsumerBehaviorHandler next)
    {
        var circuitId = context.GetCircuitBreakerId();

        if (!string.IsNullOrEmpty(circuitId))
        {
            var circuitRepository = context.ServiceProvider.GetRequiredService<ICircuitRepository>();

            var circuit = await circuitRepository.GetOrAddByIdAsync(circuitId);

            context.SetCircuitBreakerState(circuit.State);

            if (circuit.IsOpen)
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Min(5 * circuit.FailedAttempts ?? 1, 50)));
        }

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {


            throw;
        }
    }
}
