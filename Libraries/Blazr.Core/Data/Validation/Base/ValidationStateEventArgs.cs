/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core.Validation;

public class ValidationStateEventArgs : EventArgs
{
    public bool ValidationState { get; set; }
    public Guid? OjectUid { get; set; }
    public string? Field { get; set; }

    public static ValidationStateEventArgs Create(bool state, Guid? objectUid , string? field)
        => new ValidationStateEventArgs { ValidationState = state, OjectUid = objectUid, Field = field };
}

