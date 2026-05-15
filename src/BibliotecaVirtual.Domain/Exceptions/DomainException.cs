namespace BibliotecaVirtual.Domain.Exceptions;

/// <summary>
/// Excepción base para errores controlados de dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entidad, object id)
        : base($"No se encontró {entidad} con identificador '{id}'.") { }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errores { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errores)
        : base("Se produjeron errores de validación.")
    {
        Errores = errores;
    }
}
