/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core;
public interface IEditContext
{
    public event EventHandler<string?>? FieldChanged;
    public event EventHandler<bool>? EditStateUpdated;
    public Guid Uid { get; set; }
    public bool IsDirty { get; }
    public bool IsNew { get; }
}
