namespace BibliotecaVirtual.Application.Common;

/// <summary>
/// Resultado genérico para servicios de aplicación.
/// </summary>
public class Result<T>
{
    public bool Exito { get; }
    public T? Valor { get; }
    public string? Error { get; }

    private Result(bool exito, T? valor, string? error)
    {
        Exito = exito;
        Valor = valor;
        Error = error;
    }

    public static Result<T> Ok(T valor) => new(true, valor, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}
