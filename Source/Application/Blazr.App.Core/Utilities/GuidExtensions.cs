/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public static class GuidExtensions
{
    public static string ToDisplayId(this Guid value)
        => value.ToString().Substring(0, 8);
}
