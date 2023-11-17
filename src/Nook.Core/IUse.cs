namespace Nook.Core;

/// <summary>
/// Allows the use of a store.
/// </summary>
/// <typeparam name="TStore">Type of the store</typeparam>
public partial interface IUse<TStore> where TStore : IStore, new()
{
    /// <summary>
    /// Event that is raised when the store state is changed.
    /// </summary>
    public event EventHandler? StateChanged;
}
