namespace TUTSportApp.Application.Common.Models
{
    // Non-generic, static facade that owns the factories
    public static class Result
    {
        public static Result<T> Success<T>(T data) => new(true, data, null);
        public static Result<T> Failure<T>(string error) => new(false, default, error);
    }

    // The generic payload/result type has no public static members
    public sealed record Result<T>(bool IsSuccess, T? Data, string? Error);
}
