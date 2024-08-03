/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using Blazr.App.Core;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazr.App.Presentation;

public class WeatherForecastEditPresenter
{
    private readonly IDataBroker<WeatherForecastId, WeatherForecast> _dataBroker;

    public EditContext? EditContext { get; private set; }
    public WeatherForecastEditContext RecordEditContext { get; private set; }
    public IDataResult LastResult { get; private set; }
    public bool IsNew { get; private set; }

    public bool IsInvalid => this.EditContext?.GetValidationMessages().Any() ?? false;

    public WeatherForecastEditPresenter(IDataBroker<WeatherForecastId, WeatherForecast> dataBroker)
    {
        _dataBroker = dataBroker;
        this.RecordEditContext = new(new());
        this.LastResult = new DataResult(true);
    }

    public async Task LoadAsync(WeatherForecastId id)
    {
        this.IsNew = false;

        // The Update Path.  Get the requested record if it exists
        if (id.Value != Guid.Empty)
        {
            var request =new ItemQueryRequest<WeatherForecastId>(id);
            var result = await _dataBroker.GetItemAsync(request);
            if (result.Success)
            {
                RecordEditContext = new(result.Item ?? new());
                this.EditContext = new(this.RecordEditContext);
            }
            return;
        }

        // The new path.  Get a new record using the NewRecordProvider service
        this.RecordEditContext = new(this.NewWeatherForecast);
        this.EditContext = new(this.RecordEditContext);
        this.IsNew = true;
    }

    private WeatherForecast NewWeatherForecast 
        => new() { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), Id = new(Guid.NewGuid()), Summary = string.Empty, Temperature = new(0) };
    
    public async Task SaveItemAsync()
    {
        // Check for dirty record

        var record = RecordEditContext.ApplyMutation();

         ICommandRequest<WeatherForecast> command = IsNew ?
             new AddCommandRequest<WeatherForecast>(record)
            : new UpdateCommandRequest<WeatherForecast>(record); 

            this.LastResult = await _dataBroker.ExecuteCommandAsync(command);
    }
}