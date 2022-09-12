[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(Blazr.NavigationLocker.Routing.HotReloadManager))]

internal class HotReloadManager
{
    public static event Action? OnDeltaApplied;

    /// <summary>
    /// Gets a value that determines if OnDeltaApplied is subscribed to.
    /// </summary>
    public static bool IsSubscribedTo => OnDeltaApplied != null;

    /// <summary>
    /// MetadataUpdateHandler event. This is invoked by the hot reload host via reflection.
    /// </summary>
    public static void UpdateApplication(Type[]? _)
    {
        OnDeltaApplied?.Invoke();
    }
}