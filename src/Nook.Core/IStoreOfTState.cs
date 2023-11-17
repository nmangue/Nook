using Nook.Core;

/// <summary>
/// Represents a store providing a state of type <see cref="TState"/>.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public interface IStore<TState> : IStore
{
    /// <summary>
    /// Gets the current state value.
    /// </summary>
    public new TState CurrentState { get; }
}