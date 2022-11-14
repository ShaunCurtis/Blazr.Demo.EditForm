/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public record WeatherForecast
{
    public Guid Id { get; init; } = GuidExtensions.Null;
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string Summary { get; init; } = String.Empty;
}

// Keep all unnecessary clutter out of the data record
// TemperatureF ia a calculated value so doesn't belong in the base record
public static class WeatherForecastExtensions
{
    public static int TemperatureF(this WeatherForecast record)
        => 32 + (int)(record.TemperatureC / 0.5556);
}
