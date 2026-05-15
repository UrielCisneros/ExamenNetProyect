using BibliotecaVirtual.Domain.Entities;

namespace BibliotecaVirtual.Domain.Interfaces;

public interface IArchivoRepository
{
    Task<Archivo?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Archivo>> ListarPorCarpetaAsync(int carpetaId, CancellationToken ct = default);
    Task<IReadOnlyList<Archivo>> ObtenerPendientesDeBorradoFisicoAsync(DateTime limite, CancellationToken ct = default);
    Task AgregarAsync(Archivo archivo, CancellationToken ct = default);
    void Actualizar(Archivo archivo);
    void Eliminar(Archivo archivo);
}
