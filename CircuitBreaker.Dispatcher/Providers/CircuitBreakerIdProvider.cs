using CircuitBreaker.Dispatcher.Interfaces;

namespace CircuitBreaker.Dispatcher.Providers;

public class CircuitBreakerIdProvider : ICircuitBreakerIdProvider
{
    private readonly Dictionary<string, string> _registers = new Dictionary<string, string>();

    public string? GetId(string topicName)
    {
        return _registers.TryGetValue(topicName, out var id) ? id : null;
    }

    public ICircuitBreakerIdProvider Register(string topicName, string circuitId)
    {
        _registers[topicName] = circuitId;

        return this;
    }
}
