using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Application.Usuarios.Dtos;

public class UsuarioDto
{
    public string Correo { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public TipoPerfil Perfil { get; set; }
    public bool Activo { get; set; }
    public bool PuedeGestionarArchivos { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaInactivacion { get; set; }
}
