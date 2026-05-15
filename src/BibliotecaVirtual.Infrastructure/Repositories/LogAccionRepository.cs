using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;
using BibliotecaVirtual.Domain.Interfaces;
using BibliotecaVirtual.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaVirtual.Infrastructure.Repositories;

public class LogAccionRepository : ILogAccionRepository
{
    private readonly AppDbContext _ctx;
    public LogAccionRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task AgregarAsync(LogAccion log, CancellationToken ct = default)
        => await _ctx.Logs.AddAsync(log, ct);

    public async Task<IReadOnlyList<LogAccion>> ListarAsync(int top = 200, CancellationToken ct = default)
        => await _ctx.Logs.OrderByDescending(l => l.Fecha).Take(top).ToListAsync(ct);

    public async Task<(IReadOnlyList<LogAccion> items, int total)> BuscarAsync(
        string? correo,
        string? entidad,
        TipoAccionLog? accion,
        DateTime? desde,
        DateTime? hasta,
        int pagina,
        int tamano,
        CancellationToken ct = default)
    {
        var q = _ctx.Logs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(correo))
        {
            var c = correo.Trim().ToLower();
            q = q.Where(l => l.CorreoUsuario.ToLower().Contains(c));
        }
        if (!string.IsNullOrWhiteSpace(entidad))
        {
            var e = entidad.Trim().ToLower();
            q = q.Where(l => l.Entidad.ToLower() == e);
        }
        if (accion.HasValue) q = q.Where(l => l.Accion == accion.Value);
        if (desde.HasValue)  q = q.Where(l => l.Fecha >= desde.Value);
        if (hasta.HasValue)  q = q.Where(l => l.Fecha <= hasta.Value);

        var total = await q.CountAsync(ct);

        var skip = Math.Max(0, (pagina - 1) * tamano);
        var items = await q.OrderByDescending(l => l.Fecha)
            .Skip(skip).Take(tamano).ToListAsync(ct);

        return (items, total);
    }
}
