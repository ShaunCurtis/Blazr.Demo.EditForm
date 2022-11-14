/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.UI;

public partial class WeatherForecast_EditForm : Base_EditForm
{
    [Inject] private WeatherForecastViewService? ViewService { get; set; }

    private WeatherForecastEditContext RecordEditContext = new WeatherForecastEditContext(new());
    private WeatherForecastViewService viewService => this.ViewService!;

    protected async override Task OnInitializedAsync()
    {
        base.LoadState = ComponentState.Loading;
        await this.viewService.GetForecastAsync(this.Id);
        this.RecordEditContext = new WeatherForecastEditContext(this.viewService.Record);
        this.editContext = this.RecordEditContext;
        base.LoadState = ComponentState.Loaded;
    }

    private async Task SaveRecord()
    {
        await this.viewService.UpdateRecordAsync(RecordEditContext.AsRecord());
        this.RecordEditContext.Save();
    }

    private async Task AddRecord()
    => await this.viewService.AddRecordAsync(
        new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Id = Guid.NewGuid(),
            Summary = "Balmy",
            TemperatureC = 14
        });

    protected override void BaseExit()
    => this.NavManager?.NavigateTo("/weatherforecast");

}
