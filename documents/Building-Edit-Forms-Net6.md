## TL;DR

This article is the culmination of a two year effort to find a simple straighforward solution to the Dirty Form problem in Blazor.  There have been many dead end roads and half baked solutions along the way.

To cut to the chase, we finally have a built in solution in Net7.0.  If you are still on Net6 you need to read the Net6 version of this article.  This article covers how to integrate the new features into the edit form context.

## Discussion

The context of this article: there have been many discussions, articles and proposals since Blazor was first released on how to handle edit forms. Specifically how to stop, or at least warn, the user when leaving a dirty form. The problem is not specific to Blazor: all Single Page Applications and Web Sites face the same challenges.

In a classic web form, every navigation is a `get` or a `post` back to the server. We can use the Browser `window.beforeunload` event to warn a user that they have unsaved data on the page. Not great, but at least something - we'll be using it later. This technique falls flat in SPAs. What looks like a navigation event isn't. The NavigationManager intercepts any navigation attempt from the page, triggers its own LocationChanged event and sinks the request. The Router, wired into this event, does its wizardry, and loads the new component set into the page. No real browser navigation takes place, so there's nothing for the browser's beforeunload event to catch.

It's up to the programmer to write code that stops a user moving away from a dirty form. That's easier said than done when your application depends on the URL navigation pretext. Toolbars, navigation side bars and many buttons submit URLs to navigate around the application. Think of the out-of-the-box Blazor template. There's all the links in the left navigation, about in the top bar.

Personally, I have a serious issue with the whole routing charade: an SPA is an application, not a website, but I appear to be a minority of one! This article is for the majority.

The goal is to prevent navigation where possible i.e. lock the page, and where that fails hit the user with an exit option message.  No side doors!

![Dirty Editor](https://shauncurtis.github.io/articles/assets/Edit-Forms/Dirty-App-Exit.png)

## Code Repository and Demo Site

The repository for the article is [here](https://github.com/ShaunCurtis/Blazr.Demo.EditForm).

You can see the code in this article in action on my Blazr Database demo site is here - [Blazr.Demo Azure Site](https://blazr-demo.azurewebsites.net/)


## Form Exits

There are three (controlled) ways a user can exit a form:
1. **Intra Form Navigation** - Click on an Exit or other button in the form.
2. **Intra Application Navigation** - Click on a link in a navigation bar outside the form, click on the forward or back buttons on the browser.
3. **Extra Application Navigation** - entering a new Url in the address bar, clicking on a favourite, closing the browser Tab or application.

We have no control over killing the browser - a reboot or system crash - so don't consider those here.

## Blazor and Navigation

Blazor's Client side Javascript code registers event handlers for the various navigation events within the page and from the back/forward buttons.

In Blazor Web Assembly, these events surface in DotNetCore code in JsInterop code:

```csharp
[JSInvokable(nameof(NotifyLocationChanged))]
public static void NotifyLocationChanged(string uri, bool isInterceptedLink)
{
    WebAssemblyNavigationManager.Instance.SetLocation(uri, isInterceptedLink);
}
```

In Server the events get passed through the SignalR connection and surface:

```csharp
public async ValueTask OnLocationChanged(string uri, bool intercepted)
{
    var circuitHost = await GetActiveCircuitAsync();
    if (circuitHost == null)
        return;

    _ = circuitHost.OnLocationChangedAsync(uri, intercepted);
}
```

This calls the `CircuitHost` method:

```csharp
public async Task OnLocationChangedAsync(string uri, bool intercepted)
{
    await Renderer.Dispatcher.InvokeAsync(() =>
    {
        var navigationManager = (RemoteNavigationManager)Services.GetRequiredService<NavigationManager>();
        navigationManager.NotifyLocationChanged(uri, intercepted);
    });
 }
//lots of code missing to only show the relevant lines
 ```

## The Options

### The Javascript Option

One possible approach to intercept and cancel the events that drive the Blazor code.

The code block below shows one possibility.

```js
let blazr_PreventNavigation = false;  // main control bool to stop/allow navigation
let idCounter = 0;
let lastHistoryItemId = 0; //needed to detect back/forward, in popstate event handler

window.blazr_setPageLock = function (lock) {
    blazr_PreventNavigation = lock;
}

window.history.pushState = (function (basePushState) {
    return function (...args) {
        if (blazr_PreventNavigation) {
            return;
        }
        basePushState.apply(this, args);
    }
})(window.history.pushState);

window.addEventListener('beforeunload', e => {
    if (blazr_PreventNavigation) {
        e.returnValue = 'There are unsaved changes'
    }
});

window.addEventListener('load', () => {
    let resetUrlRun = false;
    function popStateListener(e) {
        if (blazr_PreventNavigation) {
            let lockNavigation = false;

            //popstate can be triggered twice, but we want to show confirm dialog only once
            lockNavigation = blazr_PreventNavigation && !resetUrlRun;

            if (lockNavigation) {
                //this will nearly always cancel Blazor navigation, but the url is already changed
                e.stopImmediatePropagation();
                e.preventDefault();
                e.returnValue = false;
            }

            if (resetUrlRun) {
                //Resets resetUrlRun on second run
                resetUrlRun = false;
            }

            // Only runs on the first pass through    
            if (lockNavigation) {
                //detect back vs forward 
                const currentId = e.state && e.state.__incrementalId;
                const navigatingForward = currentId > lastHistoryItemId;

                //revert url
                if (navigatingForward) {
                    history.back();
                }
                else {
                    history.forward();
                }
                resetUrlRun = true;
            }
        }
    }

    window.addEventListener('popstate', popStateListener, { capture: true });

    window.history.pushState = (function (basePushState) {
        return function (...args) {
            if (blazr_PreventNavigation) {
                return;
            }
            if (!args[0]) {
                args[0] = {};
            }
            lastHistoryItemId = history.state && history.state.__incrementalId;
            args[0].__incrementalId = ++idCounter; //track order of history items            
            basePushState.apply(this, args);
        }
    })(window.history.pushState);
});
```

This code works most of the time in Blazor Server, but not in Web Assembly mode.  Unfortunately, unless someone knows how to make it 100%, I've discounted it for now.  The code is available in Repo for you to test and work with. 

### The Navigation Manager Option

In Blazor Web Assembly, `NotifyLocationChanged` uses the static `WebAssemblyNavigationManager.Instance` to get the currently registered singleton instance and calls the `SetLocation` method on that instance.  Specifically, it doesn't access the instance through the DI container.  This is Web Assembly, so Scoped and Singleton are the same thing, it uses the "Singleton" pattern to access the only instance of `WebAssemblyNavigationManager` running in the browser container.

We can exploit this in the WASM solution by "unregistering" `WebAssemblyNavigationManager` as the registered `NavigationManager` in the WASM DI container, and registering our own `BlazrNavigationManager` instead.  We re-register `WebAssemblyNavigationManager` as itself.

Now all DI container services that use `NavigationManager`, register for `LocationChanged` on our `BlazrNavigationManager`.  The JSInterop events still surface though `WebAssemblyNavigationManager`, but now only `BlazrNavigationManager` is registered for the `LocationChanged` event on the `WebAssemblyNavigationManager` instance.  We now have control over whether or not we pass the event on.  Specifically, we now control `Router`.

Unfortunately, Blazor server does things differently. `OnLocationChangedAsync` gets the registered service and expects it to be `RemoteNavigationManager` which is a sealed internal class.  No exploitable options here.  We have no way to get `Router` to register with our NavigationManager!

### The Router Option

The third, and only bulletproof method, is a router that uses `BlazrNavigationManager`.  The code within the *Routing* directory and namespace does that.

`BlazrRouter` is a very lightly modified standard Router implementation.  The modifications are:

1. Inject `BlazrNavigationManager` instead of `NavigationManager`.  The router then registers with the `BlazrNavigationManager` `LocationChanged` event and thus only receives location changed events it passes through.
2. HotReload is disabled as the `HotReloadManager` is an internal class that can'r be access from outside it's parent assembly.

You can review the code in the Repo.

## Form Edit State

The first step to controlling form exits is to manage the state of the form - is the data in the form different from the record?  There's nothing in out-of-the-box Blazor to do this: there's a very simplistic attempt in `EditContext`, but it's not fit-for-purpose.   We need an edit state manager.

`EditStateContext` implements the functionality to track the state of the current edit form. 

### EditStateContext

`EditStateContext` loads content from the `EditContext` Model into a `EditFieldCollection` of `EditField` objects.  This collection is populated when the form loads.  Individual edits are passed in by calling `NotifyFieldChanged`, and the `EditedValue` property of the correct `EditField` is updated.  `EditFieldCollection` exposes a `IsDirty` property which is true if any `EditField` is dirty i.e. `Value` does not equal `EditedValue`. 

`EditStateContext` registers with `EditContext` for FieldChanges and calls `NotifyFieldChanged` whenever a field is updated.

```csharp
public class EditStateContext : IDisposable
{
    public bool IsDirty => EditFields.IsDirty;

    public event EventHandler<EditStateEventArgs>? EditStateChanged;

    private EditFieldCollection EditFields = new EditFieldCollection();

    protected EditContext? EditContext { get; private set; }

    public EditStateContext(EditContext editContext)
        => this.Load(editContext);

    public void Load(EditContext editContext)
    {
        this.EditContext = editContext;
        this.LoadEditState();
        this.EditContext.OnFieldChanged += this.FieldChanged;
    }

    private void LoadEditState()
        => this.GetEditFields();

    private void GetEditFields()
    {
        var model = this.EditContext!.Model;
        this.EditFields.Clear();
        if (model is not null)
        {
            var props = model.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.CanWrite)
                {
                    var value = prop.GetValue(model);
                    EditFields.AddField(model, prop.Name, value);
                }
            }
        }
    }
    private void FieldChanged(object? sender, FieldChangedEventArgs e)
        => this.NotifyFieldChanged(e);

    public void NotifyFieldChanged(FieldChangedEventArgs e)
    {
        var wasDirty = EditFields.IsDirty;

        // Get the PropertyInfo object for the model property
        // Uses reflection to get property and value
        var prop = e.FieldIdentifier.Model.GetType().GetProperty(e.FieldIdentifier.FieldName);
        if (prop != null)
        {
            // Get the value for the property
            var value = prop.GetValue(e.FieldIdentifier.Model);

            // Sets the edit value in the EditField
            EditFields.SetField(e.FieldIdentifier.FieldName, value);

            // Invokes EditStateChanged if changed
            var isStateChange = (EditFields.IsDirty) != wasDirty;
            if (isStateChange)
                this.NotifyEditStateChanged();
        }
    }

    public void NotifySaved()
    {
        var currentState = this.EditFields.IsDirty;
        this.LoadEditState();
        if (currentState)
            NotifyEditStateChanged();
    }

    private void NotifyEditStateChanged()
        => EditStateChanged?.Invoke(this, EditStateEventArgs.NewArgs(EditFields?.IsDirty ?? false));

    public void Dispose()
    {
        if (this.EditContext is not null)
            this.EditContext.OnFieldChanged += this.FieldChanged;
    }
}
```

#### EditStateEventArgs

```csharp
using System;

namespace Blazr.EditForms
{
    public class EditStateEventArgs : EventArgs
    {
        public bool IsDirty { get; set; }

        public static EditStateEventArgs NewArgs(bool dirtyState)
            => new EditStateEventArgs { IsDirty = dirtyState };
    }
}
```

#### EditField

`EditField` looks like this.  All but `EditedValue` are `init` record type properties.

```csharp
    public class EditField
    {
        public string FieldName { get; init; }
        public Guid GUID { get; init; }
        public object Value { get; init; }
        public object EditedValue { get; set; }
        public object Model { get; init; }

        public bool IsDirty
        {
            get
            {
                if (Value != null && EditedValue != null) return !Value.Equals(EditedValue);
                if (Value is null && EditedValue is null) return false;
                return true;
            }
        }

        public EditField(object model, string fieldName, object value)
        {
            this.Model = model;
            this.FieldName = fieldName;
            this.Value = value;
            this.EditedValue = value;
            this.GUID = Guid.NewGuid();
        }

        public void Reset()
            => this.EditedValue = this.Value;
    }
```
#### EditFieldCollection

`EditFieldCollection` implements `IEnumerable`.  It provides:
 1.  An `IsDirty` property which checks the state of all the `EditFields` in the collection.
 2. A set of getters and setters for adding and setting the edit state. 

```csharp
    public class EditFieldCollection : IEnumerable
    {
        private List<EditField> _items = new List<EditField>();
        public int Count => _items.Count;
        public Action<bool> FieldValueChanged;
        public bool IsDirty => _items.Any(item => item.IsDirty);

        public void Clear()
            => _items.Clear();

        public void ResetValues()
            => _items.ForEach(item => item.Reset());
..... lots of getters and setters and IEnumerator implementation code
```

## The Navigation Lock Code

### The NavigationLock Component

Page locking is implemented in the UI through  the `NavigationLock` component.  It looks like this:

```csharp
public class NavigationLock : ComponentBase, IDisposable
{
    [Inject] private IJSRuntime? _js { get; set; }

    [Inject] private BlazrNavigationManager? blazrNavigationManager { get; set; }    
    
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool locked;

    protected override void OnInitialized()
        => blazrNavigationManager!.BeforeLocationChange += OnBeforeLocationChange;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            SetPageLock();
        return Task.CompletedTask;
    }

    public void SetLock(bool locked)
    {
        this.locked = locked;
        this.SetPageLock();
    }

    private void OnBeforeLocationChange(object? sender, NavigationData e)
        => e.IsCanceled = this.locked;

    private void SetPageLock()
        => _js!.InvokeAsync<bool>("blazr_setPageLock", locked);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<NavigationLock>>(0);
        builder.AddAttribute(1, "Value", this);
        builder.AddAttribute(2, "ChildContent", this.ChildContent);
        builder.CloseComponent();
    }

    public void Dispose()
        => blazrNavigationManager!.BeforeLocationChange -= OnBeforeLocationChange;
}
```

It's default first load condition is unlocked.  It has a single public method `SetLock`.  This sets an internal state global variable and calls the Javascript `blazr_setPageLock` passing true/false to set or unset page locking.  

It injects the new `BlazrNavigationManager` and registers for the `OnBeforeLocationChange` event.  It sets `IsCancelled` on the `NavigationData` object it is passed to the lock state.  We'll see `BlazrNavigationManager` in a minute.

The Component UI wraps the child content in a cascade of the component.

### site.js

The client side guts of the solution.  In essence we add listeners into the various browser navigation events and either allow or prevent event propogation.  The code is shown in full below.

```js
let blazr_PreventNavigation = false;  // main control bool to stop/allow navigation

window.blazr_setPageLock = function (lock) {
    blazr_PreventNavigation = lock;
}

window.addEventListener('beforeunload', e => {
    if (blazr_PreventNavigation) {
        e.returnValue = 'There are unsaved changes in your form!'
    }
});
```

### BlazrNavigationManager

`BlazrNavigationManager` replaces the normal Navigation Manager.

It injects the standard Navigation Manager through Dependancy Injection as it's UnderlyingNavigationManager.  It registers on the `OnUnderlyingNavigationManager.LocationChanged` event.  The event handler invokes all registered delegates on it's `BeforeLocationChange` event.  If `NavigationData.IsCanceled` is true it sinks the event.  If not, it invokes it's own base class `LocationChanged` event though `NotifyLocationChanged`.

If navigation is cancelled, it instigates a second Navigation event on the standard Navigation Manager to reset the Uri. 

```csharp
public class BlazrNavigationManager : NavigationManager
{
    private NavigationManager _UnderlyingNavigationManager;
    private bool _isBlindNavigation = false;

    public event EventHandler<NavigationData>? BeforeLocationChange;

    public BlazrNavigationManager(NavigationManager? underlyingNavigationManager)
    {
        _UnderlyingNavigationManager = underlyingNavigationManager!;
        base.Initialize(underlyingNavigationManager!.BaseUri, underlyingNavigationManager.Uri);
        _UnderlyingNavigationManager.LocationChanged += OnUnderlyingNavigationManagerLocationChanged;
    }

    protected override void EnsureInitialized()
    {
        base.Initialize(_UnderlyingNavigationManager.BaseUri, _UnderlyingNavigationManager.Uri);
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        // Call the underlying navigation manager.
        _UnderlyingNavigationManager.NavigateTo(uri, forceLoad);
    }

    private NavigationData NotifyBeforeLocationChange(LocationChangedEventArgs e)
    {
        var navigation = new NavigationData()
        {
            CurrentLocation = this.Uri,
            NewLocation = e.Location,
            IsNavigationIntercepted = e.IsNavigationIntercepted,
            IsCanceled = false
        };
        BeforeLocationChange?.Invoke(this, navigation);
        return navigation;
    }

    private void OnUnderlyingNavigationManagerLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var navigation = NotifyBeforeLocationChange(e);
        // Check our blind navigation flag.  If we are blind navigating
        // we just set the flag back to false and exit
        if (_isBlindNavigation)
        {
            _isBlindNavigation = false;
            return;
        }

        // Navigation is cancelled
        // we set the flag so we don't create a loop with the dummy run
        if (navigation.IsCanceled)
        {
            // prevents a NavigateTo loop
            _isBlindNavigation = true;
            // Puts the link back - else it will change, but the page will not navigate.
            _UnderlyingNavigationManager.NavigateTo(this.Uri, false);
            return;
        }

        // Normal Navigation path
        // NOTE: We set the Uri before calling notify location changed, as it will use this uri property in its args.
        this.Uri = e.Location;
        // Trigger the Location Changed event for all listeners including the Router
        this.NotifyLocationChanged(e.IsNavigationIntercepted);
        // Belt and braces to ensure false 
        _isBlindNavigation = false;
    }
}
```

### WeatherForecastEditorForm

Our base editor form looks like this.  It's all UI controls which you can investigate in the Repo code.

```html
<UILoader State="this.LoadState">
    <FormViewTitle>
        <h2>Weather Forecast Editor</h2>
    </FormViewTitle>
    <EditForm EditContext="this.editContent">
        <UIContainer Size=BootstrapSize.Fluid>
            <UIFormRow>
                <UIColumn Columns=12 MediumColumns=6>
                    <FormEditControl Label="Date" ShowLabel="true" @bind-Value="this.viewService.EditModel.Date" ControlType="typeof(InputDate<DateTime>)" IsRequired="true" ShowValidation="true" HelperText="Enter the Forecast Date"></FormEditControl>
                </UIColumn>
                <UIColumn Columns=12 MediumColumns=6>
                    <FormEditControl Label="Temperature &deg;C" ShowLabel="true" @bind-Value="this.viewService.EditModel.TemperatureC" ControlType="typeof(InputNumber<int>)" IsRequired="true" ShowValidation="true" HelperText="Enter the Temperature"></FormEditControl>
                </UIColumn>
            </UIFormRow>
            <UIFormRow>
                <UIColumn Columns=12>
                    <FormEditControl Label="Summary" ShowLabel="true" @bind-Value="this.viewService.EditModel.Summary" IsRequired="true" ShowValidation="true" HelperText="Summarise the Weather"></FormEditControl>
                </UIColumn>
            </UIFormRow>
        </UIContainer>
        <UIContainer Size=BootstrapSize.Fluid>
            <UIFormRow>
                <UIColumn Columns=12 class="text-end">
                    <UIButton class="btn-success" Disabled="!this.isDirty" ClickEvent="this.SaveRecord">Save</UIButton>
                    <UIButton class="btn-dark" Show="!this.isDirty" ClickEvent="this.Exit">Exit</UIButton>
                    <UIButton class="btn-danger" Show="this.isDirty" ClickEvent="this.ExitWithoutSaving">Exit Without Saving</UIButton>
                </UIColumn>
            </UIFormRow>
        </UIContainer>
    </EditForm>
</UILoader>
```

And the code behind.

Most of the code is in a `BaseEditForm`.

The relevant section is in `OnInitializedAsync` where the `EditStateContext` is set up.  `OnEditStateChanged` importantly sets the lock on the Cascaded `NavigationLock`.

```csharp
public partial class WeatherForecastEditForm : BaseEditForm, IDisposable
{
    [Inject] private WeatherForecastViewService? ViewService { get; set; }

    private WeatherForecastViewService viewService => this.ViewService!;

    protected async override Task OnInitializedAsync()
    {
        base.LoadState = ComponentState.Loading;
        await this.viewService.GetForecastAsync(Id);
        base.editContent = new EditContext(this.viewService.EditModel);
        base.editStateContext = new EditStateContext(base.editContent);
        base.editStateContext.EditStateChanged += base.OnEditStateChanged;
        base.LoadState = ComponentState.Loaded;
    }

    private async Task SaveRecord()
    {
        await this.viewService.UpdateRecordAsync();
        base.editStateContext?.NotifySaved();
    }

    private async Task AddRecord()
    => await this.viewService.AddRecordAsync(
        new DcoWeatherForecast
        {
            Date = DateTime.Now,
            Id = Guid.NewGuid(),
            Summary = "Balmy",
            TemperatureC = 14
        });

    protected override void BaseExit()
    => this.NavManager?.NavigateTo("/weatherforecast");

    public void Dispose()
    {
        if (base.editStateContext is not null)
            base.editStateContext.EditStateChanged -= base.OnEditStateChanged;
    }
}
```

## The Infratructure Code
## MainLayout.razor

The most common approach is to use the component in Layouts.  Here's the modified `MainLayout`.

```csharp
<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
        </div>

        <article class="content px-4">
            <NavigationLock>
                @Body
            </NavigationLock>
        </article>
    </main>
</div>
```

The `Body` content is wrapped in the component, and the component cascaded, so is available to all pages/components that use the layout.

### _Layout.cshtml/index.html

Add the js script as the last script to load in either Layout.cshtml or index.html.

Server

```html
<script src="_framework/blazor.server.js"></script>
<script src="_content/Blazr.NavigationLocker/site.js"></script>
```

WASM

```html
<script src="_framework/blazor.webassembly.js"></script>
<script src="_content/Blazr.NavigationLocker/site.js"></script>
```

### Services

Add the required services.

Server
```csharp
    services.AddBlazrNavigationLockerServerServices();
```
WASM
```csharp
    services.AddBlazrNavigationLockerWASMServices();
```

## Using BlazrRouter

To use `BlazrRouter` simply replace the standard router in `App.razor`.

```xml
<BlazrRouter AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</BlazrRouter>
```

You will need to register the services as above.
