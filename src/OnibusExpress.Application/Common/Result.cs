namespace OnibusExpress.Application.Common;

/// <summary>
/// Resultado de uma operação de negócio. Sucesso ou falha com <see cref="Error"/>.
/// Erros de negócio são valores, não exceções.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("Resultado de sucesso não pode ter erro.");
        }

        if (!isSuccess && error is null)
        {
            throw new InvalidOperationException("Resultado de falha exige um erro.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

/// <summary>Resultado que carrega um valor em caso de sucesso.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, Error? error)
        : base(isSuccess, error) => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não há valor em um resultado de falha.");

    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}
