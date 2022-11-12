/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Data;

internal record DboWeatherForecast
{
    public Guid Id { get; init; }

    public DateOnly Date { get; init; }

    public int TemperatureC { get; init; }

    public string? Summary { get; init; }

    public DroWeatherForecast ToDto()
        => new DroWeatherForecast
        {
            Id = this.Id,
            Date = this.Date,
            TemperatureC = this.TemperatureC,
            Summary = this.Summary ?? string.Empty
        };

    public static DboWeatherForecast FromDto(DroWeatherForecast record)
        => new DboWeatherForecast
        {
            Id = record.Id,
            Date = record.Date,
            TemperatureC = record.TemperatureC,
            Summary = record.Summary
        };
}
