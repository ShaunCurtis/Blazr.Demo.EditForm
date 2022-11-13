/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core.Edit;

public record RecordPropertyState(Guid ObjectUid, string Field) { }

public class RecordPropertyStateCollection : IEnumerable<RecordPropertyState>
{
    private readonly List<RecordPropertyState> _states = new List<RecordPropertyState>();

    public void Add(RecordPropertyState state)
        => _states.Add(state);

    public void Add(Guid objectUid, string field)
        => _states.Add(new RecordPropertyState(objectUid, field));


    public void ClearState(Guid objectUid, string field)
    {
        var toDelete = _states.Where(item => item.ObjectUid.Equals(objectUid) && item.Field.Equals(field)).ToList();
        if (toDelete is not null)
            foreach (var state in toDelete)
                _states.Remove(state);
    }

    public void ClearAllstates()
        => _states.Clear();

    public bool GetState(Guid objectUid, string? field = null)
        => field is null
            ? _states.Any(item => item.ObjectUid.Equals(objectUid))
            : _states.Any(item => item.Field.Equals(field) && item.ObjectUid.Equals(objectUid));

    public bool HasStates(Guid? objectUid = null)
        => objectUid is null
            ? _states.Any()
            : _states.Any(item => item.ObjectUid.Equals(objectUid));

    public IEnumerator<RecordPropertyState> GetEnumerator()
        => _states.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
