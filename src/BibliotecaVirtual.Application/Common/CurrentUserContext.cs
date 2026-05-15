using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Application.Common;

/// <summary>
/// Información del usuario autenticado, inyectada en cada request.
/// </summary>
public class CurrentUserContext
{
    public string? Correo { get; set; }
    public TipoPerfil? Perfil { get; set; }
    public bool PuedeGestionarArchivos { get; set; }
    public bool EstaAutenticado => !string.IsNullOrWhiteSpace(Correo);
}
