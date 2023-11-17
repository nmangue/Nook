namespace Nook.Core;

/// <summary>
/// Represents a non-generic store.
/// </summary>
/// <remarks>
/// This interface exists for technical constraints. It should
/// not be used outside this library.
/// </remarks>
public interface IStore
{
    /// <summary>
    /// Gets the current state value.
    /// </summary>
    public object CurrentState { get; }

    /// <summary>
    /// Event that is raised when the <see cref="CurrentState"/> property is changed.
    /// </summary>
    public event EventHandler? StateChanged;
}
