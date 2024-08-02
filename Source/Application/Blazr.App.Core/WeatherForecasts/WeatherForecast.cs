/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecast
{
    public WeatherForecastId Id { get; set; }
    public DateOnly Date { get; set; }
    public Temperature Temperature { get; set; }
    public string? Summary { get; set; }
}
