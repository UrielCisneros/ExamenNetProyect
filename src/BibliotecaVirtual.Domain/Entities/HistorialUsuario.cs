using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Domain.Entities;

/// <summary>
/// Historial de acciones realizadas sobre un usuario (creación, inactivación, reactivación, asignación de perfil, etc.).
/// </summary>
public class HistorialUsuario
{
    public int Id { get; set; }
    public string CorreoUsuario { get; set; } = string.Empty;   // FK a Usuario.Correo
    public Usuario? Usuario { get; set; }

    public TipoAccionLog Accion { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public string CorreoEjecutor { get; set; } = string.Empty;  // quién hizo la acción
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
