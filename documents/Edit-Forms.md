# Blazor Edit Forms

> WORK IN PROGRESS

# Base Edit Form

`BaseEditForm` is an abstract class based on `ComponentBase` that implements all the boilerplate code.

It does not use the normal lifecycle `OnInitialized{Async}/OnParsmetersSet{Async}`.  These are left reserved for the actual implementation classes.  Instead it implements it's logic by overriding `SetParametersAsync`.

It registers a handler on the `NavigationManager`'s `RegisterLocationChangingHandler` and capture the `IDisposable` object returned, so it can dispose the object correctly.  The handler stops or allows navigation based on the `EditContext` state. `NavigateRegardless` provides an override for exiting without saving.  

```csharp
public abstract class BaseEditForm : ComponentBase, IDisposable
{
    [Parameter] public Guid Id { get; set; } = GuidExtensions.Null;

    [Inject] protected NavigationManager NavManager { get; set; } = default!;
    [Inject] protected IServiceProvider serviceProvider { get; set; } = default!;

    private bool _isInitialized;
    private IDisposable? registration;

    protected bool NavigateRegardless;
    protected abstract IEditContext EditContext { get; }

    protected bool IsDirty => this.EditContext?.IsDirty ?? false;

    public ComponentState LoadState { get; protected set; } = ComponentState.New;

    public async override Task SetParametersAsync(ParameterView parameters)
    {
        // Call all the normal lifecycle
        await base.SetParametersAsync(parameters);
        // Then set up the Page locking stuff
        if (!_isInitialized)
        {
            registration = NavManager.RegisterLocationChangingHandler(OnLocationChanging);
            if (this.EditContext is not null)
                this.EditContext.EditStateUpdated += this.OnStateChanged;
            
            _isInitialized = true;
        }
    }

    protected void OnStateChanged(object? sender, bool state)
        => this.InvokeAsync(StateHasChanged);

    // This is the method called by the Navigation manager to check if navigation is allowed
    protected ValueTask OnLocationChanging(LocationChangingContext changingContext)
    {
        // Test to see if we need to block Navigation
        //  Block only if we have no override and the edit context is dirty
        var noNavigation = !NavigateRegardless && this.IsDirty;

        if (noNavigation)
            changingContext.PreventNavigation();

        return ValueTask.CompletedTask;
    }

    protected void Exit()
        => BaseExit();

    protected void ExitWithoutSaving()
    {
        this.NavigateRegardless = true;
        BaseExit();
    }

    protected virtual void BaseExit()
        => this.NavManager?.NavigateTo("/");

    public void Dispose()
        => registration?.Dispose();
}
```

We can now build out `WeatherForecastEditForm`.  Note:

1. The class inherits from `BaseEditForm`.
2. We inject `WeatherForecastEditService` and call `GetForecastAsync` to get the record.
3. We set the getter on the interface based `EditContext` property to the service `EditContext`.
4. The class is `sealed` for performance. 

```csharp
public sealed partial class WeatherForecastEditForm : BaseEditForm
{
    [Inject] private WeatherForecastEditService Service { get; set; } = default!;
    private WeatherForecastEditContext RecordEditContext => this.Service.EditContext;

    protected override IEditContext EditContext => this.Service.EditContext;

    protected async override Task OnInitializedAsync()
    {
        base.LoadState = ComponentState.Loading;
        await this.Service.GetForecastAsync(this.Id);
        base.LoadState = ComponentState.Loaded;
    }

    private async Task SaveRecord()
        => await this.Service.UpdateRecordAsync(RecordEditContext.AsRecord());

    private async Task AddRecord()
        => await this.Service.AddRecordAsync(this.NewRecord);

    protected override void BaseExit()
        => this.NavManager?.NavigateTo("/weatherforecast");

    private WeatherForecast NewRecord
        => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Id = Guid.NewGuid(),
            Summary = "Balmy",
            TemperatureC = 14
        };
}
```

Some key bits in the markup:

1. We use a loader component to only renderer the full content once all the data has loaded.
2. We cascade the `IEditContext` so the form components have access to it.
3. The `NavigationLock` component, which controls extra-site navigation, is tied into the EditContext State.
4. We use the Bootstrap container and form design.
5. We use a `BlazrInput` input component. 

The markup:

```csharp
@namespace Blazr.App.UI
@inherits BaseEditForm
@implements IDisposable

<UILoader State="this.LoadState">
    <CascadingValue Value=this.EditContext>
        <FormViewTitle>
            <h2>Weather Forecast Editor</h2>
        </FormViewTitle>
        <UIContainer Size=BootstrapSize.Fluid>
            <UIFormRow>
                <UIColumn Columns=12 MediumColumns=6>
                    <label class="form-label">Date</label>
                    <BlazrInput type="date" class="form-control" FieldName="@WeatherForecastConstants.Date" FieldObjectUid="this.RecordEditContext.InstanceId" @bind-Value=this.RecordEditContext.Date />
                    <div class="form-text">Set the date for this forecast.</div>
                </UIColumn>
                <UIColumn Columns=12 MediumColumns=6>
                    <label class="form-label">Temperature &deg;C</label>
                    <BlazrInput type="number" class="form-control" FieldName="@WeatherForecastConstants.TemperatureC" FieldObjectUid="this.RecordEditContext.InstanceId" @bind-Value=this.RecordEditContext.TemperatureC />
                    <div class="form-text">Set the Temperature for this forecast.</div>
                </UIColumn>
            </UIFormRow>
            <UIFormRow>
                <UIColumn Columns=12>
                    <label class="form-label">Summary</label>
                    <BlazrInput type="text" class="form-control" FieldName="@WeatherForecastConstants.Summary" FieldObjectUid="this.RecordEditContext.InstanceId" @bind-Value=this.RecordEditContext.Summary />
                    <div class="form-text">Summarize the weather for this forecast.</div>
                </UIColumn>
            </UIFormRow>
        </UIContainer>
        <UIContainer Size=BootstrapSize.Fluid>
            <UIFormRow>
                <UIColumn Columns=12 class="text-end">
                    <UIButton class="btn-success" Disabled="!this.IsDirty" ClickEvent="this.SaveRecord">Save</UIButton>
                    <UIButton class="btn-dark" Show="!this.IsDirty" ClickEvent="this.Exit">Exit</UIButton>
                    <UIButton class="btn-danger" Show="this.IsDirty" ClickEvent="this.ExitWithoutSaving">Exit Without Saving</UIButton>
                </UIColumn>
            </UIFormRow>
        </UIContainer>
    </CascadingValue>
</UILoader>

<NavigationLock ConfirmExternalNavigation=this.IsDirty />

```


Finally we define a Route:

```csharp
@page "/weatherforecast/edit/{Id:guid}"

<WeatherForecastEditForm Id=this.Id></WeatherForecastEditForm>

@code {
    [Parameter] public Guid Id { get; set; }
}
```