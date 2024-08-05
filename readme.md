# Building Blazor Edit Forms

A set of solutions for building edit forms in Blazor.

In this article I'll look at the following challenges:

1. Prevent navigation when the edit form is dirty.

2. Manage edits and only apply them to the underlying objects/data pipeline on the save event.

3. Track the edit state:  has the editable data changed.

4. Validaate the data.

## Repo

The repo for this article is at https://github.com/ShaunCurtis/Blazr.Demo.EditForm.

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

And a `Temperature` value object:

```csharp
public readonly struct Temperature
{
    public Temperature(int value)
        => this.Value = value;

    public int Value { get; init; }

    public int Celcius => this.Value;
    public int Fahrenheit => 32 + (int)(Value / 0.5556);
}
```

## Record Edit Context

One of the traps many fall into is mutating the stored original object in the edit process.  Clicking Cancel doesn't work.  The live object has already been changed and the only way to revert to the original is to reload everything.

The simplest way to solve this is to create a record edit context.  The live edits are applied to the record edit context, and only committed to the live record on save once the record edit context has been validated.

The `WeatherForecastEditContext` looks like this.  It pulls the three editable fields out into types that can be edited [Temperature becomes an int].  THe constructor takes the live record as its input and populates the object properties.  `ApplyMutation` applies the changes to the live object.

The `[TrackState]` attribute is part of the *Blazr.EditStateTracker* package.  It labels properties the `BlazrEditStateTracker` component should track in the  `EditForm`.  We'll see the component in use later.

```csharp
public class WeatherForecastEditContext
{
    public WeatherForecast BaseRecord { get; private set; }

    // These are the Properties that can be edited
    // They will be tracked in the EditContext by the EditStateTracker
    [TrackState] public DateOnly Date { get; set; }
    [TrackState] public int Temperature { get; set; }
    [TrackState] public string? Summary { get; set; }

    public WeatherForecastEditContext(WeatherForecast record)
    {
        this.BaseRecord = record;
        this.Date = record.Date;
        this.Summary = record.Summary;
        this.Temperature = record.Temperature.Value;
    }

    public WeatherForecast ApplyMutation()
    {
        BaseRecord.Date = this.Date;
        BaseRecord.Summary = this.Summary;
        BaseRecord.Temperature = new(this.Temperature);
        return BaseRecord;
    }
}
```

## Validation

This solution uses *Blazr.Validation* which is a simple Blazor implementation of the Fluent Validation library.

In this case the validation is applied to the `WeatherForecastEditContext`, not the domain model.

```csharp
public class WeatherForecastEditContextValidator : AbstractValidator<WeatherForecastEditContext>
{
    public WeatherForecastEditContextValidator()
    {
        this.RuleFor(p => p.Summary)
            .MinimumLength(3)
            .WithState(p => p);

        this.RuleFor(p => p.Date)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Date must be in the future")
            .WithState(p => p);

        this.RuleFor(p => p.Temperature)
            .GreaterThanOrEqualTo(-60)
            .LessThanOrEqualTo(70)
            .WithState(p => p);
    }
}
```

## Edit Form

The form implements `IDisposable` to deal with event handlers it registers.  `WeatherForecastEditPresenter` provides thw interface into the data pipeline and manages all the data objects required by the form. 

```html
@namespace Blazr.App.UI.Bootstrap
@implements IDisposable
@inject WeatherForecastEditPresenter Presenter
```

The basic markup framework looks like this.  

1. `BlazrEditStateTracker` tracks the state of the model edit properties.
2. `BlazrFluentValidator` applies the `WeatherForecastEditContextValidator` to the model.   

```html
<div class="container">
    <div class="row mb-2">
        <div class="col h2">Weather Forecast Editor</div>
    </div>
    @if (this.Presenter.EditContext is not null)
    {
        <EditForm EditContext="Presenter.EditContext">

            <BlazrEditStateTracker LockNavigation="true" />
            <BlazrFluentValidator TRecord="WeatherForecastEditContext"
                            TValidator="WeatherForecastEditContextValidator" />

        //... Edit controls

            <div class="row mb-2">
                <div class="col-12 text-end ">
                    <button class="btn btn-dark" hidden="@_isDirty"
                            @onclick="this.OnExitAsync">
                        Exit
                    </button>
                    <button class="btn btn-danger" hidden="@_isClean"
                            @onclick="this.OnExitAsync">
                        Exit without Save
                    </button>
                    <button class="btn btn-primary" disabled="@_isSaveButtonDisabled"
                            @onclick="this.OnSaveAsync">
                        @_saveButtonText
                    </button>
                </div>
            </div>

        </EditForm>
    }
</div>

@if(!this.Presenter.LastResult.Success)
{
    <div class="alert alert-danger">_@this.Presenter.LastResult.Message</div>
}

<NavigationLock OnBeforeInternalNavigation="this.OnLocationChanging" ConfirmExternalNavigation="_isDirty"  />

```

1. `Close` is a cascaded delegate to call to clode the dialog control hosting the form.
2.  `WeatherForecastId` is the entity id for the record to edit.
3.  `_store` is the reference to the `BlazrEditStateTracker` store containing the edit state of the model.

The properties and fields:

```csharp
@code {
    [CascadingParameter] private Action? Close { get; set; }
    [Parameter] public WeatherForecastId WeatherForecastId { get; set; } = default!;

    private bool _isDirty;
    private bool _isClean => !_isDirty;
    private bool _isSaveButtonDisabled => _isClean && !this.Presenter.IsNew;
    private bool _stateStoreAttached;
    private string _saveButtonText => this.Presenter.IsNew ? "Add" : "Save";
    private WeatherForecastEditContext _record => this.Presenter.RecordEditContext;

    private BlazrEditStateStore? _store;
```

`OnInitializedAsync` runs when the form loads and provides the record identity for the presenter to load.

`OnAfterRender` wires up the `BlazrEditStateStore` instance and registers a handler for the `StoreUpdated` event.  `Dispose` unregisters the handler.

`OnEditStateMayHaveChanged` is the event handler.  It updates the local _isDirty state and renders the component if the state has changed..

```csharp
    protected async override Task OnInitializedAsync()
    {
        await this.Presenter.LoadAsync(this.WeatherForecastId);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        // The EditContext is only created after the first render.
        // We need to make sure the StateStore has been attached to the EditContext
        // before we try and get it.
        // The only place we can do this is in the second OnAfterRender UI event
        _store = _store ?? this.Presenter.EditContext?.GetStateStore();
        if (_store is not null && !_stateStoreAttached)
        {
            _store.StoreUpdated += OnEditStateMayHaveChanged;
            _stateStoreAttached = true;
        }
    }
    
    private void OnEditStateMayHaveChanged(object? sender, EventArgs e)
    {
        var isDirty = this.Presenter.EditContext?.GetEditState() ?? false;

        // If no state change exit
        if (isDirty == _isDirty)
            return;

        _isDirty = isDirty;
        this.StateHasChanged();
    }

    public void Dispose()
    {
        if (_store is not null)
            _store.StoreUpdated -= OnEditStateMayHaveChanged;
    }
```

Finally the Save and exit handlers.

```csharp
    private async Task OnSaveAsync()
    {
        if (!this.Presenter.EditContext?.Validate() ?? false)
            return;

        await this.Presenter.SaveItemAsync();

        if (this.Presenter.LastResult.Success)
            this.Close?.Invoke();
    }

    private Task OnExitAsync()
    {
        this.Close?.Invoke();
        return Task.CompletedTask;
    }
```

The navigation locking is handled by the `NavigationLock` component.  External navigation is controlled by `_isDirty` state field and internal by the delegate method `OnLocationChanging`.

```csharp
    private void OnLocationChanging(LocationChangingContext context)
    {
        if (_isDirty)
            context.PreventNavigation();
    }
```

## The Presenter

The presenter provides the interface to the data pipeline and the datsa used by the UI form.



```csharp
public class WeatherForecastEditPresenter
{
    private readonly IDataBroker<WeatherForecastId, WeatherForecast> _dataBroker;

    public EditContext? EditContext { get; private set; }
    public WeatherForecastEditContext RecordEditContext { get; private set; }
    public IDataResult LastResult { get; private set; }
    public bool IsNew { get; private set; }

    public bool IsInvalid => this.EditContext?.GetValidationMessages().Any() ?? false;

    public WeatherForecastEditPresenter(IDataBroker<WeatherForecastId, WeatherForecast> dataBroker)
    {
        _dataBroker = dataBroker;
        this.RecordEditContext = new(new());
        this.LastResult = new DataResult(true);
    }

    // Methods
}
```

`NewWeatherForecast` provides a new `WeatherForecast` with default values.

```csharp

    private WeatherForecast NewWeatherForecast 
        => new() { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), Id = new(Guid.NewGuid()), Summary = string.Empty, Temperature = new(0) };
```

`LoadAsync` gets the `WeatherForecast` instance from the data pipeline and creates the `EditContext` and `RecordEditContext` instances.

In the application a `WeatherForecastId` with an empty Guid value singnals a request to create a new record.  The presenter generates a real new `WeatherForecast` from the `NewWeatherForecast` property [with a valid `WeatherForecastId`].

```csharp

    public async Task LoadAsync(WeatherForecastId id)
    {
        this.IsNew = false;

        // The Update Path.  Get the requested record if it exists
        if (id.Value != Guid.Empty)
        {
            var request =new ItemQueryRequest<WeatherForecastId>(id);
            var result = await _dataBroker.GetItemAsync(request);
            if (result.Success)
            {
                RecordEditContext = new(result.Item ?? new());
                this.EditContext = new(this.RecordEditContext);
            }
            return;
        }

        // The new path.  Get a new record using the NewRecordProvider service
        this.RecordEditContext = new(this.NewWeatherForecast);
        this.EditContext = new(this.RecordEditContext);
        this.IsNew = true;
    }
```

`SaveItemAsync` checks the edit state of the model, applies the mutation to the base record, creates an appropriate command request and executes it against the data broker.

```csharp
    public async Task SaveItemAsync()
    {
        // Get the EditStateTracker store from the EditContext
        var store = this.EditContext?.GetStateStore();

        // Check for a clean record.
        if (!store?.IsDirty() ?? false)
        {
            this.LastResult = new DataResult(false, "There are no changes to save");
            return;
        }

        var record = RecordEditContext.ApplyMutation();

         ICommandRequest<WeatherForecast> command = IsNew ?
             new AddCommandRequest<WeatherForecast>(record)
            : new UpdateCommandRequest<WeatherForecast>(record); 

            this.LastResult = await _dataBroker.ExecuteCommandAsync(command);
    }
}
```
