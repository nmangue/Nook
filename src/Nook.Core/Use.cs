using Microsoft.Extensions.DependencyInjection;

namespace Nook.Core;

/// <summary>
/// Manages the store and its state.
/// </summary>
/// <typeparam name="TStore">Type of the store.</typeparam>
/// <remarks>
/// Only for the library usage.
/// </remarks>
public partial class Use<TStore> : IUse<TStore> where TStore : IStore, new()
{
    /// <summary>
    /// Gets the store instance.
    /// </summary>
    public TStore Instance { get; }

    /// <summary>
    /// Event that is raised when the store state is changed.
    /// </summary>
    public event EventHandler? StateChanged;

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Use{TStore}"</typeparamref>"/>.
    /// </summary>
    public Use(IServiceProvider serviceProvider)
    {
        Instance = new();
        Instance.StateChanged += Instance_StateChanged;
        _serviceProvider = serviceProvider;
    }

    private void Instance_StateChanged(object? sender, EventArgs e)
    {
        StateChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Creates a new <see cref="IServiceScope"/> that can be used to resolve scoped services.
    /// </summary>
    /// <returns>A <see cref="IServiceScope"/> that can be used to resolve scoped services.</returns>
    public IServiceScope CreateServiceScope()
        => _serviceProvider.CreateScope();
}
