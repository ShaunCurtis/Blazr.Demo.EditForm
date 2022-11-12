﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.Template.Tests.ViewServices
{
    public partial class WeatherForecastViewServiceTests
    {

        private ValueTask<IEnumerable<DcoWeatherForecast>> GetWeatherForecastListAsync(int noOfRecords)
            => ValueTask.FromResult(WeatherForecastDataStore.CreateTestForecasts(noOfRecords).AsEnumerable());

        private ValueTask<int> GetWeatherForecastCountAsync(int noOfRecords)
            => ValueTask.FromResult(noOfRecords);
    }
}
