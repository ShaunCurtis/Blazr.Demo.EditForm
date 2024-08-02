/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public record ItemQueryRequest<T>(T id);
public record ItemQueryResult<T>(bool Success, T? Item);
public record ListQueryRequest(int StartIndex = 0, int PageSize = 1000);
public record ListQueryResult<T>(bool Success, IEnumerable<T> Items, int TotalCount = 0);

public interface ICommandRequest<T> { T item { get; } }
public interface ICommandResult { bool Success { get; } }

public record CommandResult(bool Success) : ICommandResult;

public record AddCommandRequest<T>(T item) : ICommandRequest<T>;
public record AddCommandResult(bool Success, object Id) : ICommandResult;

public record DeleteCommandRequest<T>(T item) : ICommandRequest<T>;

public record UpdateCommandRequest<T>(T item) : ICommandRequest<T>;
