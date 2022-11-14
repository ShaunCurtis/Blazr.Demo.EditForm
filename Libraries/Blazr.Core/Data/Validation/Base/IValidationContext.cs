﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core.Validation;
public interface IValidationContext
{
    public event EventHandler<ValidationStateEventArgs>? ValidationStateUpdated;
    public bool HasMessages(Guid? objerctUid = null, string? fieldName = null);
    public IEnumerable<string> GetMessages(Guid? objerctUid = null, string? fieldName = null);
    public ValidationResult Validate(Guid? objerctUid = null, string? fieldName = null);
}