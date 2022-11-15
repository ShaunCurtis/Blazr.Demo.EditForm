/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.UI;

public sealed partial class WeatherForecastEditForm : BaseEditForm
{
    [Inject] private WeatherForecastEditService Service { get; set; } = default!;
    private WeatherForecastEditContext RecordEditContext => this.Service.EditContext;

    protected override IEditContext EditContext => this.Service.EditContext;

    protected async override Task OnInitializedAsync()
    {
        base.LoadState = ComponentState.Loading;
        await this.Service.GetForecastAsync(this.Id);
        base.LoadState = ComponentState.Loaded;
    }

    private async Task SaveRecord()
        => await this.Service.UpdateRecordAsync(RecordEditContext.AsRecord());

    private async Task AddRecord()
        => await this.Service.AddRecordAsync(this.NewRecord);

    protected override void BaseExit()
        => this.NavManager?.NavigateTo("/weatherforecast");

    private WeatherForecast NewRecord
        => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Id = Guid.NewGuid(),
            Summary = "Balmy",
            TemperatureC = 14
        };
}
