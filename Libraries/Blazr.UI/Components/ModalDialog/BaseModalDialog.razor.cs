/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.UI;

public partial class BaseModalDialog : ComponentBase, IModalDialog
{
    public ModalOptions Options { get; protected set; } = new ModalOptions();

    public bool Display { get; protected set; }

    protected RenderFragment? _Content { get; set; }

    protected string Width => this.Options.TryGet<string>(ModalOptions.__Width, out string? value) ? $"width:{value}" : string.Empty;

    protected bool ExitOnBackGroundClick => this.Options.TryGet<bool>(ModalOptions.__ExitOnBackGroundClick, out bool value) ? value : false;

    protected TaskCompletionSource<ModalResult> _ModalTask { get; set; } = new TaskCompletionSource<ModalResult>();

    public Task<ModalResult> ShowAsync<TModal>(ModalOptions options) where TModal : IComponent
    {
        this.Options = options ??= this.Options;
        this._ModalTask = new TaskCompletionSource<ModalResult>();
        this._Content = new RenderFragment(builder =>
        {
            builder.OpenComponent(1, typeof(TModal));
            builder.CloseComponent();
        });
        this.Display = true;
        InvokeAsync(StateHasChanged);
        return this._ModalTask.Task;
    }

    /// <summary>
    /// Method to update the state of the display based on UIOptions
    /// </summary>
    /// <param name="options"></param>
    public void Update(ModalOptions? options = null)
    {
        this.Options = options ??= this.Options;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Method called by the dismiss button to close the dialog
    /// sets the task to complete, show to false and renders the component (which hides it as show is false!)
    /// </summary>
    public async void Dismiss()
    {
        _ = this._ModalTask.TrySetResult(ModalResult.Cancel());
        this.Display = false;
        this._Content = null;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Method called by child components through the cascade value of this component
    /// sets the task to complete, show to false and renders the component (which hides it as show is false!)
    /// </summary>
    /// <param name="result"></param>
    public async void Close(ModalResult result)
    {
        _ = this._ModalTask.TrySetResult(result);
        this.Display = false;
        this._Content = null;
        await InvokeAsync(StateHasChanged);
    }

    private void OnBackClick(MouseEventArgs e)
    {
        if (ExitOnBackGroundClick)
            this.Close(ModalResult.Exit());
    }

}

