// Mazaad.Application/Common/Result.cs

namespace Mazaad.Application.Common
{
    public class Result<T>
    {
        public bool Succeeded { get; private set; }
        public T? Data { get; private set; }
        public string? Error { get; private set; }
        public IEnumerable<string> Errors { get; private set; } = Enumerable.Empty<string>();

        public static Result<T> Success(T data) =>
            new() { Succeeded = true, Data = data };

        public static Result<T> Failure(string error) =>
            new() { Succeeded = false, Error = error, Errors = new[] { error } };

        public static Result<T> Failure(IEnumerable<string> errors) =>
            new() { Succeeded = false, Error = errors.FirstOrDefault(), Errors = errors };
    }

    public class Result
    {
        public bool Succeeded { get; private set; }
        public string? Error { get; private set; }
        public IEnumerable<string> Errors { get; private set; } = Enumerable.Empty<string>();

        public static Result Success() => new() { Succeeded = true };

        public static Result Failure(string error) =>
            new() { Succeeded = false, Error = error, Errors = new[] { error } };

        public static Result Failure(IEnumerable<string> errors) =>
            new() { Succeeded = false, Error = errors.FirstOrDefault(), Errors = errors };
    }
}