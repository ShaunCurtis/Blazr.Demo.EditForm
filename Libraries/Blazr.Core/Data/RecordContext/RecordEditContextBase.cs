namespace Blazr.Core.Edit;

public abstract class RecordEditContextBase<TRecord> : IEditContext
    where TRecord : class, new()
{
    protected TRecord BaseRecord = new();

    // InstanceId provides a unique reference for an instance of the Edit Context
    // It'siused in validation and state tracking in `FieldReference` objects
    public Guid InstanceId { get; } = Guid.NewGuid();
    public bool ValidateOnFieldChanged { get; set; } = false;
    public bool IsDirty => !BaseRecord.Equals(this.AsRecord());
    public bool IsValid => ValidationMessages.HasMessages();
    public bool IsNew => this.Uid == Guid.Empty;

    public abstract bool IsLoaded { get; protected set; }
    public abstract Guid Uid { get; set; }

    public readonly ValidationMessageCollection ValidationMessages = new();
    public readonly PropertyStateCollection PropertyStates = new();

    public event EventHandler<string?>? FieldChanged;
    public event EventHandler<bool>? EditStateUpdated;
    public event EventHandler<ValidationStateEventArgs>? ValidationStateUpdated;

    public RecordEditContextBase() { }

    public RecordEditContextBase(TRecord record)
        => this.Load(record, false);

    public void Save()
        => this.Load(this.AsRecord());

    public abstract void Load(TRecord record, bool notify = true);
    public abstract TRecord AsRecord();
    public abstract TRecord AsNewRecord();
    public abstract void Reset();

    protected bool UpdateifChangedAndNotify<TType>(ref TType currentValue, TType value, TType originalValue, string fieldName)
    {
        if (!this.IsLoaded)
            throw RecordContextNotLoadedException.Create($"You can't set values in {this.GetType().Name} before you have loaded a record");

        var hasChanged = !value?.Equals(currentValue) ?? currentValue is not null;
        var hasChangedFromOriginal = !value?.Equals(originalValue) ?? originalValue is not null;
        if (hasChanged)
        {
            currentValue = value;
            NotifyFieldChanged(fieldName);
        }

        var field = FieldReference.Create(this.InstanceId, fieldName);
        this.PropertyStates.ClearState(field);
        if (hasChangedFromOriginal)
            this.PropertyStates.Add(field);

        return hasChanged;
    }

    public void NotifyFieldChanged(string? fieldName)
    {
        FieldChanged?.Invoke(null, fieldName);
        EditStateUpdated?.Invoke(null, IsDirty);
    }

    public bool HasMessages(FieldReference field)
        => this.ValidationMessages.HasMessages(field);

    public bool IsChanged(FieldReference field)
        => this.PropertyStates.GetState(field);

    public virtual ValidationResult Validate(FieldReference? field = null)
    {
        var result = new ValidationResult { IsValid = ValidationMessages.HasMessages(), ValidationMessages = ValidationMessages, ValidationNotRun = false };
        this.ValidationStateUpdated?.Invoke(null, ValidationStateEventArgs.Create(result.IsValid, field));
        return result;
    }
}
