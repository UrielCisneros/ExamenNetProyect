using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Interfaces;
using BibliotecaVirtual.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaVirtual.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _ctx;
    public UsuarioRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default)
        => _ctx.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo, ct);

    public Task<Usuario?> ObtenerPorCorreoConHistorialAsync(string correo, CancellationToken ct = default)
        => _ctx.Usuarios.Include(u => u.Historial)
            .FirstOrDefaultAsync(u => u.Correo == correo, ct);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(bool incluirInactivos, CancellationToken ct = default)
    {
        var q = _ctx.Usuarios.AsQueryable();
        if (!incluirInactivos) q = q.Where(u => u.Activo);
        return await q.OrderBy(u => u.NombreCompleto).ToListAsync(ct);
    }

    public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
        => await _ctx.Usuarios.AddAsync(usuario, ct);

    public void Actualizar(Usuario usuario) => _ctx.Usuarios.Update(usuario);

    public Task<bool> ExisteAsync(string correo, CancellationToken ct = default)
        => _ctx.Usuarios.AnyAsync(u => u.Correo == correo, ct);
}
