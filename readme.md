# Building Blazor Edit Forms

A set of solutions for building edit forms in Blazor.

Edit forms offer some of the greatest challenges in UI/UX design.  Blazor, along with every other web based application, has fundimental issues in the standard framework that you need to code around.

In this article I'll look at the following challenges:

1. Prevent navigation when the edit form is dirty.

2. Manage edits and only apply them to the underlying objects/data pipeline on the save event.

3. Track the edit state:  has the editable data changed.

4. Validaate the data.

## Repo

The repo for this aeticle is at https://github.com/ShaunCurtis/Blazr.Demo.EditForm.

The Demo Site is at https://blazr-editforms.azurewebsites.net/

## Solution Design and Architecture

The solution is built on Clean Design principles. Each domain has it's own project with strictly defined dependancies.

The projects are split into five groups:

1. *Libraries* are projects that may be used across multiple solutions.  These would normally be packaged once stable.

1. *Application* are solution specific libraries consistent with Clean Design.

1. *Deployments* are application deployments.  There may be WASM, Maui and Server deployments.

1. *Tests* are projects containiing application test code.

1. *Aspire* are the Microsoft Aspire framework projects.  `AppHost` is normally the solution's *startup* project.

## Data Objects

The *WeatherForecast* class looks like this:

```csharp
public class WeatherForecast
{
    public WeatherForecastId Id { get; set; }
    public DateOnly Date { get; set; }
    public Temperature Temperature { get; set; }
    public string? Summary { get; set; }
}
```

It has a strongly typed ID `WeatherForecastId` which looks like this:

```csharp
public readonly struct WeatherForecastId
{
    public WeatherForecastId(Guid id)
        => this.Value = id;

    public Guid Value { get; init; }
}
```
