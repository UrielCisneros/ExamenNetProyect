using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Domain.Entities;

/// <summary>
/// Usuario del sistema. El correo electrónico es la clave primaria (identificador único).
/// Una vez creado no puede modificarse, sólo inactivarse o reactivarse.
/// </summary>
public class Usuario
{
    public string Correo { get; set; } = string.Empty;     // PK
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public TipoPerfil Perfil { get; set; }
    public bool Activo { get; set; } = true;
    public bool PuedeGestionarArchivos { get; set; } = false; // permiso adicional al Asistente
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaInactivacion { get; set; }

    public ICollection<HistorialUsuario> Historial { get; set; } = new List<HistorialUsuario>();
}
