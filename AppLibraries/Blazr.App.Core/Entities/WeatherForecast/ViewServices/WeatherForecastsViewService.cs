
using System.Net.WebSockets;
/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public class WeatherForecastsViewService
{
    private readonly IWeatherForecastDataBroker? weatherForecastDataBroker;

    public IEnumerable<WeatherForecast>? Records { get; private set; }

    public WeatherForecastsViewService(IWeatherForecastDataBroker weatherForecastDataBroker)
        => this.weatherForecastDataBroker = weatherForecastDataBroker!;

    public async ValueTask GetForecastsAsync()
    {
        this.Records = null;
        this.ListChanged?.Invoke(this.Records, EventArgs.Empty);
        var result = await weatherForecastDataBroker!.GetWeatherForecastsAsync();
        this.Records = result.Items;
        this.ListChanged?.Invoke(this.Records, EventArgs.Empty);
    }

    public event EventHandler<EventArgs>? ListChanged;

    public async void NotifyListChanged(object? sender, EventArgs e)
    {
        await this.GetForecastsAsync();
        ListChanged?.Invoke(sender, e);
    }
}
