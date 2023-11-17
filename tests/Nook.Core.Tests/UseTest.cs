using Moq;
using NFluent;

namespace Nook.Core.Tests;

public class UseTest
{
    [Fact]
    public void StateChanged_Test()
    {
        var mock = Mock.Of<IServiceProvider>();
        var useStore = new Use<CounterStore>(mock);

        Check
            .That(useStore.Instance.CurrentState.Counter)
            .IsEqualTo(CounterState.DefaultCounterValue);

        bool eventRaised = false;
        useStore.StateChanged += (_, _) => eventRaised = true;

        useStore.Instance.CurrentState = new CounterState(33);

        Check.That(eventRaised).IsTrue();
    }
}
