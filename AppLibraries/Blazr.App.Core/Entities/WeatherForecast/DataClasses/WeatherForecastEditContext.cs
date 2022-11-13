/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public class WeatherForecastEditContext : RecordEditContextBase<DroWeatherForecast>
{
    private Guid _newId = Guid.NewGuid();

    private Guid _uid = Guid.Empty;
    public override Guid Uid
    {
        get => _uid;
        set => this.UpdateifChangedAndNotify(ref _uid, value, this.BaseRecord.Id, WeatherForecastConstants.Uid);
    }

    private string _summary = string.Empty;
    public string Summary
    {
        get => _summary;
        set => this.UpdateifChangedAndNotify(ref _summary, value, this.BaseRecord.Summary, WeatherForecastConstants.Summary);
    }

    private DateOnly _date;
    public DateOnly Date
    {
        get => _date;
        set => this.UpdateifChangedAndNotify(ref _date, value, this.BaseRecord.Date, WeatherForecastConstants.Date);
    }

    private int _temperatureC;

    public int TemperatureC
    {
        get => _temperatureC;
        set => this.UpdateifChangedAndNotify(ref _temperatureC, value, this.BaseRecord.TemperatureC, WeatherForecastConstants.Date);
    }

    public override bool IsLoaded { get; protected set; }

    public WeatherForecastEditContext() { }

    public WeatherForecastEditContext(DroWeatherForecast record) : base(record) { }

    public override void Load(DroWeatherForecast record, bool notify = true)
    {
        this.BaseRecord = record with { };
        _uid = record.Id;
        _summary = record.Summary;
        _date = record.Date;
        _temperatureC = record.TemperatureC;

        if (notify)
            this.NotifyFieldChanged(null);

        this.IsLoaded = true;
    }

    public override void Reset()
        => this.Load(this.BaseRecord with { });

    public override DroWeatherForecast AsNewRecord()
        => AsRecord() with { Id = _newId };

    public override DroWeatherForecast AsRecord()
        => new DroWeatherForecast
        {
            Id = _uid,
            Summary = _summary,
            Date = _date,
            TemperatureC = _temperatureC
        };
}
