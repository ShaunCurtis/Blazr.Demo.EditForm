/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.Demo.EditForm.UI;

public partial class WeatherForecastEditForm : BaseEditForm, IDisposable
{
    [Inject] private WeatherForecastViewService? ViewService { get; set; }

    private WeatherForecastViewService viewService => ViewService!;

    protected async override Task OnInitializedAsync()
    {
        this.LoadState = ComponentState.Loading;
        await ViewService!.GetForecastAsync(Id);
        editContent = new EditContext(this.ViewService.EditModel);
        editStateContext = new EditStateContext(editContent);
        editStateContext.EditStateChanged += OnEditStateChanged;
        this.LoadState = ComponentState.Loaded;
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

    protected override void BaseExit()
    => this.NavManager?.NavigateTo("/weatherforecast");

    public void Dispose()
    {
        if (editStateContext is not null)
            editStateContext.EditStateChanged -= OnEditStateChanged;
    }
}
