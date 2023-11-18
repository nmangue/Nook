using Nook.Core;

namespace BlazorIntegration.Store;

public class CounterState
{
    public int Counter { get; }

    public CounterState(int counter)
    {
        Counter = counter;
    }

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
