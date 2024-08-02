/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public readonly struct WeatherForecastId
{
    public WeatherForecastId(Guid id)
        => this.Value = id;

    public Guid Value { get; init; }
}
