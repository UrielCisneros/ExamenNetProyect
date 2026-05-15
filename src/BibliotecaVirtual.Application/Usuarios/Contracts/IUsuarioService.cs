using BibliotecaVirtual.Application.Usuarios.Dtos;

namespace BibliotecaVirtual.Application.Usuarios.Contracts;

/// <summary>
/// Servicio para la administración de usuarios. Sólo accesible para perfiles
/// Administración de Biblioteca y Gerente de Universidad.
/// </summary>
public interface IUsuarioService
{
    Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<UsuarioDto>> ListarAsync(bool incluirInactivos, CancellationToken ct = default);
    Task<UsuarioDto> ObtenerAsync(string correo, CancellationToken ct = default);
    Task<UsuarioDto> InactivarAsync(string correo, string motivo, CancellationToken ct = default);
    Task<UsuarioDto> ReactivarAsync(string correo, string motivo, CancellationToken ct = default);
    Task<UsuarioDto> ActualizarPermisosAsync(string correo, OtorgarPermisosDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<HistorialUsuarioDto>> HistorialAsync(string correo, CancellationToken ct = default);
}
