/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public readonly struct Temperature
{
    public Temperature(int value)
        => this.Value = value;

    public int Value { get; init; }

    public int Celcius => this.Value;
    public int Fahrenheit => 32 + (int)(Value / 0.5556);
}
