using BibliotecaVirtual.Domain.Common;
using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Domain.Entities;

/// <summary>
/// Archivo (documento, video o audio) almacenado en una carpeta.
/// El borrado es lógico; la eliminación física ocurre por proceso en segundo plano.
/// </summary>
public class Archivo : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;          // máx 50
    public string Descripcion { get; set; } = string.Empty;     // máx 500
    public string RutaFisica { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public TipoArchivo Tipo { get; set; }

    public int CarpetaId { get; set; }
    public Carpeta? Carpeta { get; set; }

    public string CorreoCreador { get; set; } = string.Empty;
    public DateTime? FechaEliminacionLogica { get; set; }       // marca el inicio del temporizador
}
