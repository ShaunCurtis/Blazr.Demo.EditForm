/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecastEditContext
{
    public WeatherForecast BaseRecord { get; private set; }
    public DateOnly Date { get; set; }
    public Temperature Temperature { get; set; }
    public string? Summary { get; set; }

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
