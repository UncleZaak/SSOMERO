using System;

namespace AcademicSystem.Application.Common.Models
{
    public sealed class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; private set; }
        public T? Value { get; private set; }

        private Result()
        {
        }

        public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };
        public static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error };
    }
}
