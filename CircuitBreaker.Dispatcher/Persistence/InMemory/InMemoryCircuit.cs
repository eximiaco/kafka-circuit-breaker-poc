using CircuitBreaker.Dispatcher.Policies;

namespace CircuitBreaker.Dispatcher.Persistence.InMemory;

public class InMemoryCircuit : ICircuit
{
    public string Id { get; private set; }

    public ECircuitState State { get; private set; }

    public bool IsOpen => State == ECircuitState.Open;

    public bool IsHalfOpen => State == ECircuitState.HalfOpen;

    public bool IsClosed => State == ECircuitState.Closed;

    public int? FailedAttempts { get; private set; }

    public InMemoryCircuit(string id)
    {
        Id = id;
        State = ECircuitState.Closed;
    }

    public Task AttemptAsync(CancellationToken cancellationToken = default)
    {
        State = ECircuitState.HalfOpen;
        FailedAttempts = null;
        return Task.CompletedTask;
    }

    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        State = ECircuitState.Closed;
        FailedAttempts = null;
        return Task.CompletedTask;
    }


    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        State = ECircuitState.Open;
        return Task.CompletedTask;
    }

    public Task RegisterFail(int failedAttempts)
    {
        State = ECircuitState.Open;
        FailedAttempts = failedAttempts;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}
