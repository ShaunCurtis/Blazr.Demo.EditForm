using System.Runtime.CompilerServices;

namespace Blazr.UI;

internal static class RenderTreeBuilderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfNotNullOrEmpty(this RenderTreeBuilder builder, int sequence, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            builder.AddAttribute(sequence, name, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfTrue(this RenderTreeBuilder builder, int sequence, bool isTrue, string name, string? value)
    {
        if (isTrue)
            builder.AddAttribute(sequence, name, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfTrue(this RenderTreeBuilder builder, int sequence, bool isTrue, string name, bool? value)
    {
        if (isTrue)
            builder.AddAttribute(sequence, name, value);
    }
}
