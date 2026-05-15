using BibliotecaVirtual.Domain.Entities;

namespace BibliotecaVirtual.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorCorreoConHistorialAsync(string correo, CancellationToken ct = default);
    Task<IReadOnlyList<Usuario>> ListarAsync(bool incluirInactivos, CancellationToken ct = default);
    Task AgregarAsync(Usuario usuario, CancellationToken ct = default);
    void Actualizar(Usuario usuario);
    Task<bool> ExisteAsync(string correo, CancellationToken ct = default);
}
