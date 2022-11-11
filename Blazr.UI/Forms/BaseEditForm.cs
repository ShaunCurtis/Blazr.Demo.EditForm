using Microsoft.JSInterop;
/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.UI;

public partial class BaseEditForm : ComponentBase, IDisposable
{
    private bool _isInitialized;
    private IDisposable? registration;
    protected EditContext? editContent;
    protected bool AllowNavigationOverride;

    [Parameter] public Guid Id { get; set; } = GuidExtensions.Null;

    [Parameter] public EventCallback ExitAction { get; set; }

    [CascadingParameter] public IModalDialog? Modal { get; set; }

    [Inject] protected NavigationManager NavManager { get; set; } = default!;

    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    public ComponentState LoadState { get; protected set; } = ComponentState.New;

    protected bool IsModal => this.Modal != null;

    protected EditStateContext? editStateContext;

    protected bool IsDirty => editStateContext!.IsDirty;

    public override Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        if (!_isInitialized)
            registration = NavManager.RegisterLocationChangingHandler(OnLocationChanging);
        return base.SetParametersAsync(ParameterView.Empty);
    }

    protected void OnRecordChanged(object? sender, EventArgs e)
        => this.InvokeAsync(StateHasChanged);

    protected ValueTask OnLocationChanging(LocationChangingContext changingContext)
    {
        if (!this.AllowNavigationOverride || this.IsDirty)
            changingContext.PreventNavigation();

        return ValueTask.CompletedTask;
    }

    private async Task OnBeforeInternalNavigation(LocationChangingContext context)
    {
        string message = "This form has unsaved data. Are you sure you want to exit?";
        var isConfirmed = await JSRuntime.InvokeAsync<bool>("confirm", message);

        if (!isConfirmed)
        {
            context.PreventNavigation();
        }
    }

    protected async void Exit()
        => await DoExit();

    protected async void ExitWithoutSaving()
    {
        this.AllowNavigationOverride = true;
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
