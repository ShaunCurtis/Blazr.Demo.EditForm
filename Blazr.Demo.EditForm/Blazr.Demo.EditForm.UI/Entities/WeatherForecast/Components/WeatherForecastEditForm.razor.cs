using Blazr.NavigationLocker;

namespace Blazr.Demo.EditForm.UI;

public partial class WeatherForecastEditForm : ComponentBase
{
    private EditContext? editContent;

    [Parameter] public Guid Id { get; set; } = GuidExtensions.Null;

    [CascadingParameter] private NavigationLock? navigationLock { get; set; }

    [Inject] private WeatherForecastViewService? ViewService { get; set; }

    [Inject] private NavigationManager? NavManager { get; set; }

    public ComponentState LoadState { get; private set; } = ComponentState.New;

    private WeatherForecastViewService viewService => ViewService!;

    private EditStateContext? editStateContext;

    private bool isDirty => editStateContext!.IsDirty;

    protected async override Task OnInitializedAsync()
    {
        this.LoadState = ComponentState.Loading;
        await ViewService!.GetForecastAsync(Id);
        editContent = new EditContext(this.ViewService.EditModel);
        editStateContext = new EditStateContext(editContent);
        editStateContext.EditStateChanged += OnEditStateChanged;
        this.LoadState = ComponentState.Loaded;
    }

    private void OnRecordChanged(object? sender, EventArgs e)
        => this.InvokeAsync(StateHasChanged);

    private void OnEditStateChanged(object? sender, EditStateEventArgs e)
    {
        navigationLock?.SetLock(e.IsDirty);
        this.InvokeAsync(StateHasChanged);
    }

    private async Task SaveRecord()
    {
        await this.ViewService!.UpdateRecordAsync();
        editStateContext?.NotifySaved();
    }

    private async Task AddRecord()
    => await this.ViewService!.AddRecordAsync(
        new DcoWeatherForecast
        {
            Date = DateTime.Now,
            Id = Guid.NewGuid(),
            Summary = "Balmy",
            TemperatureC = 14
        });

    private void Exit()
    {
        this.NavManager!.NavigateTo("/weatherforecasts");
    }
    private void ExitWithoutSaving()
    {
        navigationLock?.SetLock(false);
        this.Exit();
    }

    public void Dispose()
    {
        if (editStateContext is not null)
            editStateContext.EditStateChanged -= OnEditStateChanged;
    }

}

