using NFluent;

namespace Nook.Core.Tests;

public class StoreTest
{
    [Fact]
    public void StateChanged_IsNotified()
    {
        var store = new CounterStore();
        bool eventRaised = false;
        store.StateChanged += (_, e) => eventRaised = true;

        Check.That(store.CurrentState.Counter).IsEqualTo(CounterState.DefaultCounterValue);

        const int updatedCounterValue = 42;
        store.CurrentState = new CounterState(updatedCounterValue);

        Check.That(eventRaised).IsTrue();
    }
}

public record CounterState(int Counter)
{
    public const int DefaultCounterValue = 1;

    public CounterState() : this(DefaultCounterValue)
    {
        // Nothing to do
    }
}

public class CounterStore : Store<CounterState>
{
}