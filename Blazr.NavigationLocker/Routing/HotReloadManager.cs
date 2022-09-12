// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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