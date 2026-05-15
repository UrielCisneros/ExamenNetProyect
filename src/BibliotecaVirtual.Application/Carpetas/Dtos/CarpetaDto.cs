namespace BibliotecaVirtual.Application.Carpetas.Dtos;

public class CarpetaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? CarpetaPadreId { get; set; }
    public bool EsRaiz { get; set; }
    public string? Banco { get; set; }
    public string CorreoCreador { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}

/// <summary>
/// Vista de árbol: una carpeta con sus subcarpetas y conteo de archivos.
/// </summary>
public class CarpetaArbolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsRaiz { get; set; }
    public string? Banco { get; set; }
    public int CantidadArchivos { get; set; }
    public List<CarpetaArbolDto> Subcarpetas { get; set; } = new();
}
