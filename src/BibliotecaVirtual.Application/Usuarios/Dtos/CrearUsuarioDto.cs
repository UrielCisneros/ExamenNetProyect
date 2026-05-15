using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Application.Usuarios.Dtos;

/// <summary>
/// Datos para registrar un nuevo usuario en el sistema.
/// El correo es el identificador único (PK) y no podrá modificarse luego.
/// </summary>
public class CrearUsuarioDto
{
    public string Correo { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public TipoPerfil Perfil { get; set; }
}
