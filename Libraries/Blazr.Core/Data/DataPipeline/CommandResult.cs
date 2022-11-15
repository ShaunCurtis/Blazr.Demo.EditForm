/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Core;

public record CommandResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; }

    private CommandResult(bool success, string message)
    {
        this.IsSuccess  = success;
        this.Message = message;
    }

    public static CommandResult Success()
        => new CommandResult(true, string.Empty);

    public static CommandResult Failure(string message)
        => new CommandResult(false, message);
}
