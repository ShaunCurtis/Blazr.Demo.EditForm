namespace Blazr.Core.Edit;

/// <summary>
/// Base class for all Record Edit Contexts
/// An example would be WeatherForecastEditContext
/// </summary>
/// <typeparam name="TRecord"></typeparam>
public abstract class RecordEditContextBase<TRecord> : IEditContext
    where TRecord : class, new()
{
    // Field to hold the original record from which the edit properties were loaded
    // Defaults to new as it can't be null
    protected TRecord BaseRecord = new();

    // InstanceId provides a unique reference for an instance of the Edit Context
    // It's used in validation and state tracking in `FieldReference` objects
    public Guid InstanceId { get; } = Guid.NewGuid();
    
    // Property to indicate if any of the properties have changed
    public bool IsDirty => !BaseRecord.Equals(this.AsRecord());

    // Property to indicate if the properties are currently valiud
    public bool IsValid => ValidationMessages.HasMessages();

    // Property to indicate if the record is currently a new record
    // We use Guid.Empty to indicate a new record.
    public bool IsNew => this.Uid == Guid.Empty;

    // Property used to track whether this instacne has been loaded
    public bool IsLoaded { get; protected set; }
    
    // The record Guid
    public abstract Guid Uid { get; set; }

    // The Validation message collection iused to track validation
    public readonly ValidationMessageCollection ValidationMessages = new();
    
    // The Property State collection used to track individual property state
    public readonly PropertyStateCollection PropertyStates = new();

    // Event raised whenever a field value is changed
    public event EventHandler<string?>? FieldChanged;
    
    // Event raised whenever the edit state may have been modified
        public event EventHandler<bool>? EditStateUpdated;
    
    // Event raised whenever the validation state may have changed
    public event EventHandler<ValidationStateEventArgs>? ValidationStateUpdated;

    // Ctor requires a record
    public RecordEditContextBase(TRecord record)
        => this.Load(record, false);

    // Method to update the base record to the current state
    public void Save()
        => this.Load(this.AsRecord());

    // Method to load the edut properties from the supplied record
    // must be implemented in the child class as only it knows what properties need loading
    // Always ensure you update `BaseRecord`
    public abstract void Load(TRecord record, bool notify = true);
    
    // Method to return a record based on the current state
    // Must be implemented in the child class
    public abstract TRecord AsRecord();

    // Method to return a New record based on the current state
    // Will normally use with and set a new Guid.
    // Must be implemented in the child class
    public abstract TRecord AsNewRecord();

    // Method to set the editable properties base to the BaseRecord state
    public void Reset()
        => this.Load(this.BaseRecord);

    // Method used by all the properties to do all the work associated with a editable property change
    // It checks if the property has both changed from the last value and changed from the original value in `BaseRecord`
    // Based on the results it updates the private field, updates `PropertyStates`, and raises the necessary events.
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

    // Method to raise events
    public void NotifyFieldChanged(string? fieldName)
    {
        FieldChanged?.Invoke(null, fieldName);
        EditStateUpdated?.Invoke(null, IsDirty);
    }

    // Interface Method to expose directly the `HasMessages` method in ValidationMessages
    public bool HasMessages(FieldReference field)
        => this.ValidationMessages.HasMessages(field);

    // Interface Method to expose directly the `IsChanged` method in PropertyStates
    public bool IsChanged(FieldReference field)
        => this.PropertyStates.GetState(field);

    // Interface Method to call validation
    // Should be implemented in the child class if validation is required
    public virtual ValidationResult Validate(FieldReference? field = null)
    {
        var result = new ValidationResult { IsValid = ValidationMessages.HasMessages(), ValidationMessages = ValidationMessages, ValidationNotRun = false };
        this.ValidationStateUpdated?.Invoke(null, ValidationStateEventArgs.Create(result.IsValid, field));
        return result;
    }
}
