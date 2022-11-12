/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core;

public abstract class RecordEditContextBase<TRecord> : IEditContext
    where TRecord : class, new()
{
    protected TRecord BaseRecord = new();

    public Guid InstanceId { get; } = Guid.NewGuid();

    public virtual Guid Uid { get; set; }

    public bool ValidateOnFieldChanged { get; set; } = false;

    public virtual TRecord Record => new();

    public virtual bool IsDirty => !BaseRecord.Equals(this.Record);

    public bool IsNew => this.Uid == Guid.Empty;

    public TRecord CleanRecord => this.BaseRecord;

    public abstract TRecord AsRecord();

    public event EventHandler<string?>? FieldChanged;
    public event EventHandler<bool>? EditStateUpdated;

    public RecordEditContextBase() { }

    public RecordEditContextBase(TRecord record)
        => this.Load(record);

    public abstract void Load(TRecord record);

    public abstract void Reset();

    protected bool UpdateifChangedAndNotify<TType>(ref TType currentValue, TType value, string fieldName)
    {
        var hasChanged = !value?.Equals(currentValue) ?? currentValue is not null;
        if (hasChanged)
        {
            currentValue = value;
            NotifyFieldChanged(fieldName);
        }

        return hasChanged;
    }

    public void NotifyFieldChanged(string fieldName)
    {
        FieldChanged?.Invoke(null, fieldName);
        EditStateUpdated?.Invoke(null, IsDirty);
    }
}
