namespace Nook.Core;

/// <summary>
/// Represents a store that handles a state of type <see cref="TState"/>.
/// </summary>
/// <typeparam name="TState">Type of the state value.</typeparam>
public abstract class Store<TState> : IStore<TState> where TState : new()
{
    private TState _currentState;

    /// <summary>
    /// Gets the current state value.
    /// </summary>
    public TState CurrentState
    {
        get => _currentState;
        set
        {
            if (!ReferenceEquals(CurrentState, value))
            {
                _currentState = value;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    object IStore.CurrentState => CurrentState!;

    /// <summary>
    /// Event that is raised when the <see cref="CurrentState"/> property is changed.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Store{TState}"</typeparamref>"/> class with a default state value.
    /// </summary>
    public Store()
    {
        _currentState = new();
    }
}
