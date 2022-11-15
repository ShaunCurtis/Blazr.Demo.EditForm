/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core.Edit;

public interface IEditContext
{
    public event EventHandler<string?>? FieldChanged;
    public event EventHandler<bool>? EditStateUpdated;
    public event EventHandler<ValidationStateEventArgs>? ValidationStateUpdated;
    public bool IsDirty { get; }
    public bool IsValid { get; }
    public bool IsNew { get; }

    public bool HasMessages(FieldReference field);
    public bool IsChanged(FieldReference field);
    public ValidationResult Validate(FieldReference? field);
}
