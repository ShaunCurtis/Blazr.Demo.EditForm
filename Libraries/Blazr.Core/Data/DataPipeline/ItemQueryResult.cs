/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core;

public record ItemQueryResult<TRecord>
{
    public TRecord? Item { get; init; }
    public bool IsSuccess { get; init; }
    public string Message { get; init; }

    private ItemQueryResult(TRecord? item, bool success, string message)
    {
        this.Item = item;
        this.IsSuccess  = success;
        this.Message = message;
    }

    public static ItemQueryResult<TRecord> Success(TRecord item)
        => new ItemQueryResult<TRecord>(item, true, string.Empty);

    public static ItemQueryResult<TRecord> Failure(string message)
        => new ItemQueryResult<TRecord>(default(TRecord), false, message);
}
