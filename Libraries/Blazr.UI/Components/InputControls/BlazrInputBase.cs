﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.UI;

public class BlazrInputBase<TValue> : ComponentBase
{
    [CascadingParameter] protected IEditContext editContext { get; set; } = default!;

    [Parameter, EditorRequired] public string? FieldName { get; set; }
    [Parameter] public string? Type { get; set; }
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    protected bool NoValidation;

    protected string CssClass
        => new CSSBuilder()
        .AddClassFromAttributes(AdditionalAttributes)
        .AddClass(this.editContext is not null && !this.NoValidation, ValidationCss)
        .Build();

    protected string ValidationCss
        => this.RecordEditContext?.HasMessages(this.FieldName) ?? false
        ? "is-invalid"
        : "is-valid";
        
    protected virtual string? ValueAsString
        => GetValueAsString(this.Value);

    protected override ValueTask<bool> OnParametersChangedAsync(bool firstRender)
    {
        if (firstRender && RecordEditContext is not null)
            this.RecordEditContext.ValidationStateUpdated += this.OnValidationStateUpdated;

        return ValueTask.FromResult(true);
    }

    protected void OnChanged(ChangeEventArgs e)
    {
        if (BindConverter.TryConvertTo<TValue>(e.Value, System.Globalization.CultureInfo.InvariantCulture, out TValue? result))
            this.ValueChanged.InvokeAsync(result);
    }

    protected void OnValidationStateUpdated(object? sender, ValidationStateEventArgs e)
        => this.StateHasChanged();

    protected void OnFieldChanged(object? sender, string? field)
       => this.StateHasChanged();

    public void Dispose()
    {
        if (RecordEditContext is not null)
            this.RecordEditContext.ValidationStateUpdated -= this.OnValidationStateUpdated;
    }

    protected static string? GetValueAsString(TValue? initialValue) =>
        initialValue switch
        {
            string @value => value,
            int @value => BindConverter.FormatValue(value),
            long @value => BindConverter.FormatValue(value),
            short @value => BindConverter.FormatValue(value),
            float @value => BindConverter.FormatValue(value),
            double @value => BindConverter.FormatValue(value),
            decimal @value => BindConverter.FormatValue(value),
            TimeOnly @value => BindConverter.FormatValue(value, format: "HH:mm:ss"),
            DateOnly @value => BindConverter.FormatValue(value, format: "yyyy-MM-dd"),
            DateTime @value => BindConverter.FormatValue(value, format: "yyyy-MM-ddTHH:mm:ss"),
            DateTimeOffset @value => BindConverter.FormatValue(value, format: "yyyy-MM-ddTHH:mm:ss"),
            _ => initialValue?.ToString() ?? throw new InvalidOperationException($"Unsupported type {initialValue?.GetType()}")
        };
}
