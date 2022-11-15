# The Record Edit Context

If your data classes are based on records you need a context in which to create a new record or edit an existing one.  This is the *RecordEditContext*.

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

    public void ClearAllstates()
        => _states.Clear();

    public bool GetState(FieldReference field)
        => _states.Any(item => item.Equals(field));

    public bool HasStates(Guid? objectUid = null)
        => objectUid is null
            ? _states.Any()
            : _states.Any(item => item.ObjectUid.Equals(objectUid));

    public IEnumerator<FieldReference> GetEnumerator()
        => _states.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
```

### context

```csharp
namespace Blazr.Core.Edit;

public abstract class RecordEditContextBase<TRecord> : IEditContext
    where TRecord : class, new()
{
    protected TRecord BaseRecord = new();

    // InstanceId provides a unique reference for an instance of the Edit Context
    // It's used in validation and state tracking in `FieldReference` objects
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
```
