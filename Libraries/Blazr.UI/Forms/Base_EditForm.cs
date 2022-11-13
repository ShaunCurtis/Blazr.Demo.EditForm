/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.UI;

public abstract class Base_EditForm : ComponentBase, IDisposable
{
    [Parameter] public Guid Id { get; set; } = GuidExtensions.Null;
    [Parameter] public EventCallback ExitAction { get; set; }
    [CascadingParameter] public IModalDialog? Modal { get; set; }

    [Inject] protected NavigationManager NavManager { get; set; } = default!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    private bool _isInitialized;
    private IDisposable? registration;

    protected bool NavigateRegardless;
    protected IEditContext? editContext;

    protected bool IsModal => this.Modal != null;
    protected bool IsDirty => editContext?.IsDirty ?? false;

    public ComponentState LoadState { get; protected set; } = ComponentState.New;

    public async override Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
        if (!_isInitialized)
        {
            registration = NavManager.RegisterLocationChangingHandler(OnLocationChanging);
            if (editContext is not null)
                editContext.EditStateUpdated += this.OnStateChanged;
            
            _isInitialized = true;
        }
    }

    protected void OnStateChanged(object? sender, bool state)
        => this.InvokeAsync(StateHasChanged);

    //protected void OnRecordChanged(object? sender, EventArgs e)
    //    => this.InvokeAsync(StateHasChanged);

    protected ValueTask OnLocationChanging(LocationChangingContext changingContext)
    {
        // Test to see if we need to block Navigation
        //  Block only if we have no override and the edit context is dirty
        var noNavigation = !NavigateRegardless && this.IsDirty;

        if (noNavigation)
            changingContext.PreventNavigation();

        return ValueTask.CompletedTask;
    }

    protected async void Exit()
        => await DoExit();

    protected async void ExitWithoutSaving()
    {
        this.NavigateRegardless = true;
        await DoExit();
    }

    protected virtual async Task DoExit()
    {
        // If we're in a modal context, call Close on the cascaded Modal object
        if (this.Modal is not null)
            this.Modal.Close(ModalResult.OK());
        // If there's a delegate registered on the ExitAction, execute it. 
        else if (ExitAction.HasDelegate)
            await ExitAction.InvokeAsync();
        // else fallback action is to navigate to root
        else
            this.BaseExit();
    }

    protected virtual void BaseExit()
        => this.NavManager?.NavigateTo("/");

    public void Dispose()
        => registration?.Dispose();
}
