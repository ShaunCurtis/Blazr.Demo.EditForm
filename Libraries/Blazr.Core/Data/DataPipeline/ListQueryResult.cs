/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core;

public record ListQueryResult<TRecord>
{
    public IEnumerable<TRecord> Items { get; init; }
    public bool IsSuccess { get; init; }
    public string Message { get; init; }

    private ListQueryResult(IEnumerable<TRecord> item, bool success, string message)
    {
        this.Items= item;
        this.IsSuccess  = success;
        this.Message = message;
    }

    public static ListQueryResult<TRecord> Success(IEnumerable<TRecord> items)
        => new ListQueryResult<TRecord>(items, true, string.Empty);

    public static ListQueryResult<TRecord> Failure(string message)
        => new ListQueryResult<TRecord>(Enumerable.Empty<TRecord>(), false, message);
}
