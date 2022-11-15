# Blazor Edit Forms

> WORK IN PROGRESS

## TL;DR

This article details a two year journey to find a simple straighforward solution to the Dirty Form problem in Blazor.  There have been many dead end roads and half baked solutions along the way.

To cut to the chase, we finally have a Blazor out-of-the-box solution in Net7.0.  If you're still on Net6 you need to read the Net6 version of this article.  This article covers how to integrate the new features into the edit form context.

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
