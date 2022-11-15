## Records and Edit State

The first step to controlling form exits is to manage the state of the form - is the data in the form different from the record?  There's nothing in out-of-the-box Blazor to do this: there's a very simplistic attempt in `EditContext`, but it's not fit-for-purpose.

### Records

Take a step back.  We now have records in C# and they are a godsend in data management.  I use them everywhere.  My default object for data is a `record`.

In the solution Weather Forecast looks like this.  A minimal object with an id field.

```csharp
public record WeatherForecast
{
    public Guid Id { get; init; } = GuidExtensions.Null;
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string Summary { get; init; } = String.Empty;
}
```
The calculated properties are provided through an extension startic class:

```csharp
public static class WeatherForecastExtensions
{
    public static int TemperatureF(this WeatherForecast record)
        => 32 + (int)(record.TemperatureC / 0.5556);
}
```

### Editing Records

Having switched to data class records we need an edit framework to edit them.

This is relatively easy to solve.

`EditStateContext` implements the functionality to track the state of the current edit form.

