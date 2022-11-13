/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public class WeatherForecastValidator
{
    public static ValidationResult Validate(DroWeatherForecast record, Guid objectUid, ValidationMessageCollection? validationMessages, string? fieldname = null)
    {
        ValidationState validationState = new ValidationState();

        ValidationMessageCollection messages = validationMessages ?? new ValidationMessageCollection();

        if (fieldname != null)
            validationMessages?.ClearMessages(fieldname);

        if (WeatherForecastConstants.Date.Equals(fieldname) || fieldname is null)
            record.Date.Validation(objectUid, WeatherForecastConstants.Date, messages, validationState)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now), true, "The weather forecast must be for a future date")
                .Validate(fieldname);

        if (WeatherForecastConstants.TemperatureC.Equals(fieldname) || fieldname is null)
            record.TemperatureC.Validation(objectUid, WeatherForecastConstants.TemperatureC, messages, validationState)
                .GreaterThan(-61, "The minimum Temperatore is -60C")
                .LessThan(61, "The maximum temperature is 60C")
                .Validate(fieldname);

        if (WeatherForecastConstants.Summary.Equals(fieldname) || fieldname is null)
            record.Summary?.Validation(objectUid, WeatherForecastConstants.Summary, messages, validationState)
                .ShorterThan(3, "You must select a weather summary")
                .Validate(fieldname);

        return new ValidationResult { ValidationMessages = messages, IsValid = validationState.IsValid };
    }
}
