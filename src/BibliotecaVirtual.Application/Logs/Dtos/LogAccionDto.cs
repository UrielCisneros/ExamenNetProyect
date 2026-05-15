using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Application.Logs.Dtos;

public class LogAccionDto
{
    public int Id { get; set; }
    public string CorreoUsuario { get; set; } = string.Empty;
    public TipoAccionLog Accion { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string? IdEntidad { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

/// <summary>
/// Filtros opcionales para consultar la bitácora.
/// </summary>
public class LogQueryDto
{
    public string? Correo { get; set; }
    public string? Entidad { get; set; }
    public TipoAccionLog? Accion { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 50;
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
}
