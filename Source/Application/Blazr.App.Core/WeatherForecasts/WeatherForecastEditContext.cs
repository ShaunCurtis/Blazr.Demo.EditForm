/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecastEditContext
{
    public WeatherForecast BaseRecord { get; private set; }

    // These are the Properties that can be edited
    // They will be tracked in the EditContext by the EditStateTracker
    [TrackState] public DateOnly Date { get; set; }
    [TrackState] public Temperature Temperature { get; set; }
    [TrackState] public string? Summary { get; set; }

    public WeatherForecastEditContext(WeatherForecast record)
    {
        this.BaseRecord = record;
        this.Date = record.Date;
        this.Summary = record.Summary;
        this.Temperature = record.Temperature;
    }

    public WeatherForecast ApplyMutation()
    {
        BaseRecord.Date = this.Date;
        BaseRecord.Summary = this.Summary;
        BaseRecord.Temperature = this.Temperature;
        return BaseRecord;
    }
}
