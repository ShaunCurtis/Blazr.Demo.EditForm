/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.NavigationLocker;

public class NavigationLock : ComponentBase, IDisposable
{
    [Inject] private IJSRuntime? _js { get; set; }

    [Inject] private BlazrNavigationManager? blazrNavigationManager { get; set; }    
    
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool locked;

    protected override void OnInitialized()
        => blazrNavigationManager!.BeforeLocationChange += OnBeforeLocationChange;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            SetPageLock();
        return Task.CompletedTask;
    }

    public void SetLock(bool locked)
    {
        this.locked = locked;
        this.SetPageLock();
    }

    private void OnBeforeLocationChange(object? sender, NavigationData e)
        => e.IsCanceled = this.locked;

    private void SetPageLock()
        => _js!.InvokeAsync<bool>("blazr_setPageLock", locked);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<NavigationLock>>(0);
        builder.AddAttribute(1, "Value", this);
        builder.AddAttribute(2, "ChildContent", this.ChildContent);
        builder.CloseComponent();
    }

    public void Dispose()
        => blazrNavigationManager!.BeforeLocationChange -= OnBeforeLocationChange;
}
