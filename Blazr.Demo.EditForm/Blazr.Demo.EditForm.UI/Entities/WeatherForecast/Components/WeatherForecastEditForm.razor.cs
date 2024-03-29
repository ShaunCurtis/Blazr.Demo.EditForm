﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.Demo.EditForm.UI;

public partial class WeatherForecastEditForm : BaseEditForm, IDisposable
{
    [Inject] private WeatherForecastViewService? ViewService { get; set; }

    private WeatherForecastViewService viewService => this.ViewService!;

    protected async override Task OnInitializedAsync()
    {
        base.LoadState = ComponentState.Loading;
        await this.viewService.GetForecastAsync(Id);
        base.editContent = new EditContext(this.viewService.EditModel);
        base.editStateContext = new EditStateContext(base.editContent);
        base.editStateContext.EditStateChanged += base.OnEditStateChanged;
        base.LoadState = ComponentState.Loaded;
    }

    private async Task SaveRecord()
    {
        await this.viewService.UpdateRecordAsync();
        base.editStateContext?.NotifySaved();
    }

    private async Task AddRecord()
    => await this.viewService.AddRecordAsync(
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
        if (base.editStateContext is not null)
            base.editStateContext.EditStateChanged -= base.OnEditStateChanged;
    }
}
