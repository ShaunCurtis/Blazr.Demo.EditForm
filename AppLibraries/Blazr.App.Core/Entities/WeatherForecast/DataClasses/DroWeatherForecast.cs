/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public record DroWeatherForecast
{
    public Guid Id { get; init; } = GuidExtensions.Null;

    public DateOnly Date { get; init; }

    public int TemperatureC { get; init; }

    public string Summary { get; init; } = String.Empty;
}

// Keep all unnecessary clutter out of the data record
// TemperatureF ia a calculated value so doesn't belong in the base record
public static class DroWeatherForecastExtensions
{
    public static int TemperatureF(this DroWeatherForecast record)
        => 32 + (int)(record.TemperatureC / 0.5556);
}
