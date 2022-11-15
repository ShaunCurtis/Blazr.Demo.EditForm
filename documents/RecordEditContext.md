# The Record Edit Context

Once you move to record based data classes, you need a context in which to create a new record or edit an existing one.  This is the *RecordEditContext*.

The basic process is to create class with properties that represent the editable fields in your record.  The new or load method populates the properties from the record and then saves a copy of the record.  This `BaseRecord` represents thr original state of the record.  At any point we can construct a record that represents the current state of the context `AsRecord()` and use equality checking to find the `IsDirty` state.  There are a few more bells and whistles such as tracking which fields have changed and managing validation.

The base implementation consists of two objects.  

1. `IEditContext` defines the generic properties and methods provided by `RecordEditContext` implementations.
2. `RecordEditContextBase<TRecord>` implements the `IEditContext` and provides all the generic boilerplate code.

### FieldReference

`FieldReference` is an object used to define a specific field.  It uses Guids to track instances to decouple the record from the actual object instance.  It's a  `FieldIdentifier` with decoupling from the source object.

```csharp
public record FieldReference
{
    public Guid ObjectUid { get; init; }
    public string FieldName { get; init; }

    public FieldReference(Guid objectUid, string fieldName)
    {
        this.ObjectUid = objectUid;
        this.FieldName = fieldName;
    }

    public static FieldReference Create(Guid objectUid, string fieldName)
        => new(objectUid, fieldName);
}
```

### IEditContext

Most of this is self-explanatory. `IEditContext` can be cascaded in an edit form and abstracts the functionality needed by components such as `FieldValidationMessage` which displays the validation messages for a specific field.    

```csharp
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
```

### PropertyStateCollection

`PropertyStateCollection` is a standard `IEnumerable` implementation for managing a collection of `FieldReference` objects.  It's purpose is to hold a set of `FieldReference` objects that represent properties that have changed from their original values.  We'll see it in action in the edit context.

The solution uses the collection to modify the Css on `input` controls.

```csharp
public class PropertyStateCollection : IEnumerable<FieldReference>
{
    private readonly List<FieldReference> _states = new List<FieldReference>();

    public void Add(FieldReference state)
        => _states.Add(state);

    public void Add(Guid objectUid, string field)
        => _states.Add(new FieldReference(objectUid, field));

    public void ClearState(FieldReference field)
    {
        var toDelete = _states.Where(item => item.Equals(field)).ToList();
        if (toDelete is not null)
            foreach (var state in toDelete)
                _states.Remove(state);
    }

    public void ClearAllStates()
        => _states.Clear();

    public bool GetState(FieldReference field)
        => _states.Any(item => item.Equals(field));

    public bool HasState(Guid? objectUid = null)
        => objectUid is null
            ? _states.Any()
            : _states.Any(item => item.ObjectUid.Equals(objectUid));

    public IEnumerator<FieldReference> GetEnumerator()
        => _states.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
```

### RecordEditContextBase

`RecordEditContextBase` is the base abstract class for record edit context classes.

This is a generic class where `TRecord` is the data `record`.

Read the comments for the details on specific functionality.

```csharp
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
```

## Real Implementation

We can now look at a real implementation.

First a struct to hold some constants.  I use constants because I only want to use reflection where I have to and performance doesn't matter.  Everywhere else I avoid it.  A little bit of extra time up front in development saves a lot of CPU cycles when the system is running in production.

```csharp
public readonly struct WeatherForecastConstants
{
    public const string Uid = "Uid";
    public const string Date = "Date";
    public const string TemperatureC = "TemperatureC";
    public const string Summary = "Summary";
}
```

A reminder on what `WeatherForecast` looks like:

```csharp
public record WeatherForecast
{
    public Guid Id { get; init; } = GuidExtensions.Null;
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string Summary { get; init; } = String.Empty;
}
```

And the `WeatherForecastEditContext` implementation.

Note:
1. A classic old style Property declaration for each property.
2. Each Property setter calls `UpdateifChangedAndNotify` which encapsulates the checking, updating and notifying logic.
3. Implementation of the abstract methods to load and set records

```csharp
public class WeatherForecastEditContext : RecordEditContextBase<WeatherForecast>
{
    private Guid _newId = Guid.NewGuid();

    private Guid _uid = Guid.Empty;
    public override Guid Uid
    {
        get => _uid;
        set => this.UpdateifChangedAndNotify(ref _uid, value, this.BaseRecord.Id, WeatherForecastConstants.Uid);
    }

    private string _summary = string.Empty;
    public string Summary
    {
        get => _summary;
        set => this.UpdateifChangedAndNotify(ref _summary, value, this.BaseRecord.Summary, WeatherForecastConstants.Summary);
    }

    private DateOnly _date;
    public DateOnly Date
    {
        get => _date;
        set => this.UpdateifChangedAndNotify(ref _date, value, this.BaseRecord.Date, WeatherForecastConstants.Date);
    }

    private int _temperatureC;

    public int TemperatureC
    {
        get => _temperatureC;
        set => this.UpdateifChangedAndNotify(ref _temperatureC, value, this.BaseRecord.TemperatureC, WeatherForecastConstants.TemperatureC);
    }

    public WeatherForecastEditContext(WeatherForecast record) : base(record) { }

    public override void Load(WeatherForecast record, bool notify = true)
    {
        this.BaseRecord = record with { };
        _uid = record.Id;
        _summary = record.Summary;
        _date = record.Date;
        _temperatureC = record.TemperatureC;

        if (notify)
            this.NotifyFieldChanged(null);

        this.IsLoaded = true;
    }

    public override WeatherForecast AsRecord()
        => new WeatherForecast
        {
            Id = _uid,
            Summary = _summary,
            Date = _date,
            TemperatureC = _temperatureC
        };

    public override WeatherForecast AsNewRecord()
        => AsRecord() with { Id = _newId };
}
```
## Usage

Record Edit Contexts are normally used either in Form components or in view services associated with a form.