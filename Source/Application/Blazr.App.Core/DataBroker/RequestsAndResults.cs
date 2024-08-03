/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public interface IDataResult
{
    public bool Success { get; }
    public string? Message { get; }
}

public record DataResult(bool Success, string? Message = null) : IDataResult;

public record ItemQueryRequest<T>(T id);
public record ItemQueryResult<T>(bool Success, T? Item);

public record ListQueryRequest(int StartIndex = 0, int PageSize = 1000);
public record ListQueryResult<T>(bool Success, IEnumerable<T> Items, int TotalCount = 0, string? Message = null) : IDataResult;

public interface ICommandRequest<T> { T item { get; } }
public interface ICommandResult : IDataResult { }

public record CommandResult(bool Success, string? Message = null) : ICommandResult, IDataResult;

public record AddCommandRequest<T>(T item) : ICommandRequest<T>;
public record AddCommandResult(bool Success, object Id, string? Message = null) : ICommandResult, IDataResult;

public record DeleteCommandRequest<T>(T item) : ICommandRequest<T>;

public record UpdateCommandRequest<T>(T item) : ICommandRequest<T>;
