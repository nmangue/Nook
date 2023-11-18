using Nook.Core;

namespace BlazorIntegration.Store;

public record CounterState(int Counter)
{
    public CounterState() : this(0)
    {
        // Rien à faire
    }
}

public class CounterStore : Store<CounterState>
{
    [Action]
    public CounterState Increment()
        => new(CurrentState.Counter + 1);
}
