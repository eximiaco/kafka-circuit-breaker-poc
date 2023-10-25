namespace CircuitBreaker.Dispatcher.Interfaces;

public interface ICircuitBreakerIdProvider
{
    string? GetId(string topicName);

    ICircuitBreakerIdProvider Register(string topicName, string circuitId);
}
