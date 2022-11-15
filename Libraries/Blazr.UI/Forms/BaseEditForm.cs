/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.UI;

public abstract class BaseEditForm : ComponentBase, IDisposable
{
    [Parameter] public Guid Id { get; set; } = GuidExtensions.Null;

    [Inject] protected NavigationManager NavManager { get; set; } = default!;

    private bool _isInitialized;
    private IDisposable? registration;

    protected bool NavigateRegardless;
    protected abstract IEditContext EditContext { get; }

    protected bool IsDirty => this.EditContext?.IsDirty ?? false;

    public ComponentState LoadState { get; protected set; } = ComponentState.New;

    public async override Task SetParametersAsync(ParameterView parameters)
    {
        // Call all the normal lifecycle
        await base.SetParametersAsync(parameters);
        // Then set up the Page locking stuff
        if (!_isInitialized)
        {
            registration = NavManager.RegisterLocationChangingHandler(OnLocationChanging);
            if (this.EditContext is not null)
                this.EditContext.EditStateUpdated += this.OnStateChanged;
            
            _isInitialized = true;
        }
    }

    protected void OnStateChanged(object? sender, bool state)
        => this.InvokeAsync(StateHasChanged);


    // This is the method called by the Navigation manager to check if navigation is allowed
    protected ValueTask OnLocationChanging(LocationChangingContext changingContext)
    {
        // Test to see if we need to block Navigation
        //  Block only if we have no override and the edit context is dirty
        var noNavigation = !NavigateRegardless && this.IsDirty;

        if (noNavigation)
            changingContext.PreventNavigation();

        return ValueTask.CompletedTask;
    }

    protected void Exit()
        => BaseExit();

    protected void ExitWithoutSaving()
    {
        this.NavigateRegardless = true;
        BaseExit();
    }

    protected virtual void BaseExit()
        => this.NavManager?.NavigateTo("/");

    public void Dispose()
        => registration?.Dispose();
}
