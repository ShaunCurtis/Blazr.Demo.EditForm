﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core.Edit;

public abstract class RecordEditContextBase<TRecord> : IEditContext
    where TRecord : class, new()
{
    protected TRecord BaseRecord = new();
    protected readonly RecordPropertyStateCollection RecordPropertyStateCollection = new();

    public Guid InstanceId { get; } = Guid.NewGuid();

    public abstract Guid Uid { get; set; }

    public bool ValidateOnFieldChanged { get; set; } = false;

    public virtual bool IsDirty => !BaseRecord.Equals(this.AsRecord());

    public bool IsNew => this.Uid == Guid.Empty;

    public abstract bool IsLoaded { get; protected set; }

    public TRecord CleanRecord => this.BaseRecord;

    public abstract TRecord AsRecord();

    public abstract TRecord AsNewRecord();

    public void Save()
        => this.Load(this.AsRecord());

    public event EventHandler<string?>? FieldChanged;
    public event EventHandler<bool>? EditStateUpdated;

    public RecordEditContextBase() { }

    public RecordEditContextBase(TRecord record)
        => this.Load(record, false);

    public abstract void Load(TRecord record, bool notify = true);

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

        this.RecordPropertyStateCollection.ClearState(this.InstanceId, fieldName);
        if (hasChangedFromOriginal)
            this.RecordPropertyStateCollection.Add(this.InstanceId, fieldName);

        return hasChanged;
    }

    public void NotifyFieldChanged(string? fieldName)
    {
        FieldChanged?.Invoke(null, fieldName);
        EditStateUpdated?.Invoke(null, IsDirty);
    }
}
