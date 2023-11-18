using Microsoft.AspNetCore.Components;
using Nook.Core;

namespace Nook.AspNetCore;

public abstract class ComponentBase<TStore>
    : ComponentBase where TStore : IStore, new()
{
    [Microsoft.AspNetCore.Components.Inject]
    protected IUse<TStore> Store { get; private set; } = default!;

    protected override void OnInitialized()
    {
        Store.StateChanged += Store_StateChanged;
    }

    private void Store_StateChanged(object? sender, EventArgs e)
    {
        if (ShouldRerender())
        {
            InvokeAsync(StateHasChanged);
        }
    }

    protected virtual bool ShouldRerender() => true;
}
