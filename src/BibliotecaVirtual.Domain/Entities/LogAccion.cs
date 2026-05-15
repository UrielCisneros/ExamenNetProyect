using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Domain.Entities;

/// <summary>
/// Bitácora general de acciones del sistema.
/// </summary>
public class LogAccion
{
    public int Id { get; set; }
    public string CorreoUsuario { get; set; } = string.Empty;
    public TipoAccionLog Accion { get; set; }
    public string Entidad { get; set; } = string.Empty;   // "Usuario", "Carpeta", "Archivo"
    public string? IdEntidad { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
