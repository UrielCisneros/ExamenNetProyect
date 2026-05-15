using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Interfaces;
using BibliotecaVirtual.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaVirtual.Infrastructure.Repositories;

public class ArchivoRepository : IArchivoRepository
{
    private readonly AppDbContext _ctx;
    public ArchivoRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<Archivo?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => _ctx.Archivos.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Archivo>> ListarPorCarpetaAsync(int carpetaId, CancellationToken ct = default)
        => await _ctx.Archivos.Where(a => a.CarpetaId == carpetaId && a.Activo).OrderBy(a => a.Nombre).ToListAsync(ct);

    public async Task<IReadOnlyList<Archivo>> ObtenerPendientesDeBorradoFisicoAsync(DateTime limite, CancellationToken ct = default)
        => await _ctx.Archivos
            .Where(a => !a.Activo && a.FechaEliminacionLogica != null && a.FechaEliminacionLogica <= limite)
            .ToListAsync(ct);

    public async Task AgregarAsync(Archivo archivo, CancellationToken ct = default)
        => await _ctx.Archivos.AddAsync(archivo, ct);

    public void Actualizar(Archivo archivo) => _ctx.Archivos.Update(archivo);

    public void Eliminar(Archivo archivo) => _ctx.Archivos.Remove(archivo);
}
