# Input Controls

> WORK IN PROGRESS

It's liberating to step outside the `EditForm`/`EditContext`/`InputBase` world.  You can go back to `input`.  There are consequences in the feature set, so in this section we'll build those features we need back in.

### BlazrInputBase

`BlazrInputBase<TValue>` is our base component.  It basically encapsulates most of the normal functionality of an input control and data binding  with some Css formatting.

Notes:

1. `FieldName` and `FieldObjectUid` provide the data needed to construct a `FieldReference` object to interact with the state and validation stores.
2. `Value` and `ValueChanged` are part of the standard bind setup.


```csharp
public class BlazrInputBase<TValue> : ComponentBase
{
    [CascadingParameter] protected IEditContext editContext { get; set; } = default!;
    [Parameter, EditorRequired] public string FieldName { get; set; } = string.Empty;
    [Parameter, EditorRequired] public Guid FieldObjectUid { get; set; } = Guid.Empty;
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
    {
        get
        {
            var field = FieldReference.Create(this.FieldObjectUid, this.FieldName);
            var isInvalid = this.editContext?.HasMessages(field) ?? false;
            var isChanged = this.editContext?.IsChanged(field) ?? false;

            if (isChanged && isInvalid)
                return "is-invalid";

            if (isChanged && !isInvalid)
                return "is-valid";

            return string.Empty;
        }
    }

    protected virtual string? ValueAsString
        => GetValueAsString(this.Value);

    protected override Task OnInitializedAsync()
    {
        if (this.editContext is not null)
            this.editContext.ValidationStateUpdated += this.OnValidationStateUpdated;

        return Task.CompletedTask;
    }

    protected void OnChanged(ChangeEventArgs e)
    {
        if (BindConverter.TryConvertTo<TValue>(e.Value, System.Globalization.CultureInfo.InvariantCulture, out TValue? result))
            this.ValueChanged.InvokeAsync(result);
    }

    protected void OnValidationStateUpdated(object? sender, ValidationStateEventArgs e)
        => this.StateHasChanged();

    public void Dispose()
    {
        if (this.editContext is not null)
            this.editContext.ValidationStateUpdated -= this.OnValidationStateUpdated;
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
```

Once we have this we can define a normal `input` control  like this which will handle almost everything but selects.

```csharp
public class BlazrInput<TValue> : BlazrInputBase<TValue>
{
    [DisallowNull] public ElementReference? Element { get; protected set; }

    protected RenderFragment BaseInputControl;

    public BlazrInput()
        : base()
        => this.BaseInputControl = BuildControl;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
        => this.BuildControl(builder);

    private void BuildControl(RenderTreeBuilder builder)
    {
        var isTextArea = this.Type?.Equals("textarea", StringComparison.InvariantCultureIgnoreCase) ?? false;
        var tag = isTextArea
            ? "textarea"
            : "input";

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, this.AdditionalAttributes);
        builder.AddAttributeIfTrue(2, !isTextArea, "type", this.Type);
        builder.AddAttributeIfNotNullOrEmpty(3, "class", this.CssClass);
        builder.AddAttribute(4, "value", this.ValueAsString);
        builder.AddAttribute(5, "onchange", this.OnChanged);
        builder.AddElementReferenceCapture(6, __inputReference => this.Element = __inputReference);
        builder.CloseElement();
    }
}
```

And a `select` like this:

```csharp
public class BlazrSelect<TValue> : BlazrInputBase<TValue>
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [DisallowNull] public ElementReference? Element { get; protected set; }

    private readonly bool _isMultipleSelect;
    protected RenderFragment BaseInputControl;

    public BlazrSelect()
        : base()
    {
        _isMultipleSelect = typeof(TValue).IsArray;
        this.BaseInputControl = BuildControl;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
        => this.BuildControl(builder);

    private void BuildControl(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "select");
        builder.AddMultipleAttributes(1, this.AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", this.CssClass);
        builder.AddAttributeIfTrue(3, _isMultipleSelect, "multiple", _isMultipleSelect);
        builder.AddAttribute(4, "value", this.ValueAsString);
        builder.AddAttribute(5, "onchange", this.OnChanged);
        builder.AddElementReferenceCapture(6, __inputReference => this.Element = __inputReference);
        builder.AddContent(7, this.ChildContent);
        builder.CloseElement();
    }
}
```

