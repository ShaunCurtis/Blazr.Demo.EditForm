/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.Template.Tests.ViewServices
{
    public partial class WeatherForecastViewServiceTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(55, 55)]
        [InlineData(233, 233)]
        public async void WeatherForecastViewServiceShouldGetWeatherForecastsAsync(int noOfRecords, int expectedCount)
        {
            // define
            var dataBrokerMock = new Mock<IWeatherForecastDataBroker>();
            WeatherForecastsViewService? weatherForecastsViewService = new WeatherForecastsViewService(weatherForecastDataBroker: dataBrokerMock.Object);

            dataBrokerMock.Setup(item =>
                item.GetWeatherForecastsAsync())
               .Returns(this.GetWeatherForecastListAsync(noOfRecords));
            object? eventSender = null;
            object? eventargs = null;
            weatherForecastsViewService.ListChanged += (sender, e) => { eventSender = sender; eventargs = e; };

            // test
            await weatherForecastsViewService.GetForecastsAsync();

            // assert
            Assert.IsType<List<DcoWeatherForecast>?>(weatherForecastsViewService.Records);
            Assert.Equal(expectedCount, weatherForecastsViewService.Records!.Count);
            Assert.IsType<List<DcoWeatherForecast>?>(eventSender);
            Assert.IsType<EventArgs>(eventargs);
            dataBrokerMock.Verify(item => item.GetWeatherForecastsAsync(), Times.Once);
            dataBrokerMock.VerifyNoOtherCalls();
        }
    }
}
