using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Interfaces;
using BibliotecaVirtual.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaVirtual.Infrastructure.Repositories;

public class CarpetaRepository : ICarpetaRepository
{
    private readonly AppDbContext _ctx;
    public CarpetaRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<Carpeta?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => _ctx.Carpetas.FirstOrDefaultAsync(c => c.Id == id && c.Activo, ct);

    public async Task<IReadOnlyList<Carpeta>> ListarRaicesAsync(CancellationToken ct = default)
        => await _ctx.Carpetas.Where(c => c.EsRaiz && c.Activo)
            .OrderBy(c => c.Banco).ThenBy(c => c.Nombre).ToListAsync(ct);

    public async Task<IReadOnlyList<Carpeta>> ListarPorPadreAsync(int padreId, CancellationToken ct = default)
        => await _ctx.Carpetas.Where(c => c.CarpetaPadreId == padreId && c.Activo)
            .OrderBy(c => c.Nombre).ToListAsync(ct);

    public async Task<IReadOnlyList<Carpeta>> ListarTodasActivasAsync(CancellationToken ct = default)
        => await _ctx.Carpetas.Where(c => c.Activo).ToListAsync(ct);

    public Task<bool> ExisteNombreEnPadreAsync(string nombre, int? padreId, int? exceptoId, CancellationToken ct = default)
        => _ctx.Carpetas.AnyAsync(c =>
            c.Activo
            && c.CarpetaPadreId == padreId
            && c.Nombre.ToLower() == nombre.ToLower()
            && (exceptoId == null || c.Id != exceptoId), ct);

    public async Task<bool> TieneSubcarpetasOArchivosActivosAsync(int id, CancellationToken ct = default)
    {
        var tieneSub = await _ctx.Carpetas.AnyAsync(c => c.CarpetaPadreId == id && c.Activo, ct);
        if (tieneSub) return true;
        return await _ctx.Archivos.AnyAsync(a => a.CarpetaId == id && a.Activo, ct);
    }

    public async Task AgregarAsync(Carpeta carpeta, CancellationToken ct = default)
        => await _ctx.Carpetas.AddAsync(carpeta, ct);

    public void Actualizar(Carpeta carpeta) => _ctx.Carpetas.Update(carpeta);
}
