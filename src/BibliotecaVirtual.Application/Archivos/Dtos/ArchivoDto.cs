using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Application.Archivos.Dtos;

public class ArchivoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public TipoArchivo Tipo { get; set; }
    public int CarpetaId { get; set; }
    public string CorreoCreador { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
    public DateTime? FechaEliminacionLogica { get; set; }
}

public class ActualizarArchivoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class DescargaArchivoDto
{
    public Stream Contenido { get; set; } = Stream.Null;
    public string NombreOriginal { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
}
