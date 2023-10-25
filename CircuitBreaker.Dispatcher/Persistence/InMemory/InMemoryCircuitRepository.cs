using System.Collections.Concurrent;
using CircuitBreaker.Dispatcher.Policies;

namespace CircuitBreaker.Dispatcher.Persistence.InMemory;

public class InMemoryCircuitRepository : ICircuitRepository
{
    private readonly ConcurrentDictionary<string, InMemoryCircuit> _list = new();

    public Task<ICircuit> GetOrAddByIdAsync(string id)
    {
        return Task.FromResult<ICircuit>(_list.GetOrAdd(id, (id) =>
        {
            return new InMemoryCircuit(id);
        }));
    }
}
