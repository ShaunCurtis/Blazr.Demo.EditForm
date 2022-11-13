/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.UI;

public class BlazrInputNumber<TValue> : BlazrInputBase<TValue>
{
    [DisallowNull] public ElementReference? Element { get; protected set; }

    protected RenderFragment BaseInputControl;
    private bool _isDecimal;
    private TValue? _oldValue;
    private string? _stringValue;

    public BlazrInputNumber()
        : base()
    {
        this.BaseInputControl = BuildControl;
        _isDecimal = IsDecimal(this.Value); 
    }

    protected override ValueTask<bool> OnParametersChangedAsync(bool firstRender)
    {
        if (firstRender)
        {
            _oldValue = this.Value;
            _stringValue = GetValueAsString(this.Value);
        }
        return ValueTask.FromResult(true);
    }

    protected override string? ValueAsString
        => GetNumberAsString(this.Value);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
        => this.BuildControl(builder);

    private void BuildControl(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddMultipleAttributes(1, this.AdditionalAttributes);
        builder.AddAttribute(2, "type", "text");
        builder.AddAttributeIfNotNullOrEmpty(3, "class", this.CssClass);
        builder.AddAttribute(4, "value", _stringValue);
        builder.AddAttribute(5, "oninput", this.OnInput);
        builder.AddElementReferenceCapture(6, __inputReference => this.Element = __inputReference);
        builder.CloseElement();
    }

    protected async Task OnInput(ChangeEventArgs e)
    {
        var isNumberOrNull = BindConverter.TryConvertTo<TValue>(e.Value, System.Globalization.CultureInfo.InvariantCulture, out TValue? result)
            || string.IsNullOrEmpty(e.Value?.ToString() ?? string.Empty);

        //TODO - need to chack if a point is allowed
        // good to go 
        if (isNumberOrNull)
        {
            _oldValue = result;
            _stringValue = this.Value is null
                ? string.Empty
                : GetValueAsString(_oldValue);
            await this.ValueChanged.InvokeAsync(result);
            return;
        }

        _stringValue = null;
        this.StateHasChanged();
        await Task.Delay(1);
        _stringValue = _oldValue is null
            ? string.Empty
            : GetValueAsString(_oldValue);
        this.StateHasChanged();
    }

    protected static bool IsDecimal(TValue? initialValue) =>
    initialValue switch
    {
        int @value => false,
        long @value => false,
        short @value => false,
        float @value => true,
        double @value => true,
        decimal @value => true,
        _ => throw new InvalidOperationException($"Unsupported type {initialValue?.GetType()}")
    };

    protected static string? GetNumberAsString(TValue? initialValue) =>
    initialValue switch
    {
        string @value => value,
        int @value => BindConverter.FormatValue(value),
        long @value => BindConverter.FormatValue(value),
        short @value => BindConverter.FormatValue(value),
        float @value => BindConverter.FormatValue(value),
        double @value => BindConverter.FormatValue(value),
        decimal @value => BindConverter.FormatValue(value),
        _ => initialValue?.ToString() ?? throw new InvalidOperationException($"Unsupported type {initialValue?.GetType()}")
    };
}
