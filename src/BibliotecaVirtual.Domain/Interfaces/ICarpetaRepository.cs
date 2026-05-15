using BibliotecaVirtual.Domain.Entities;

namespace BibliotecaVirtual.Domain.Interfaces;

public interface ICarpetaRepository
{
    Task<Carpeta?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Carpeta>> ListarRaicesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Carpeta>> ListarPorPadreAsync(int padreId, CancellationToken ct = default);
    Task<IReadOnlyList<Carpeta>> ListarTodasActivasAsync(CancellationToken ct = default);
    Task<bool> ExisteNombreEnPadreAsync(string nombre, int? padreId, int? exceptoId, CancellationToken ct = default);
    Task<bool> TieneSubcarpetasOArchivosActivosAsync(int id, CancellationToken ct = default);
    Task AgregarAsync(Carpeta carpeta, CancellationToken ct = default);
    void Actualizar(Carpeta carpeta);
}
