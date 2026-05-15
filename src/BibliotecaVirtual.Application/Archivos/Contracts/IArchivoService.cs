using BibliotecaVirtual.Application.Archivos.Dtos;

namespace BibliotecaVirtual.Application.Archivos.Contracts;

public interface IArchivoService
{
    Task<ArchivoDto> SubirAsync(
        int carpetaId,
        string nombre,
        string descripcion,
        string nombreOriginal,
        Stream contenido,
        long tamanoBytes,
        CancellationToken ct = default);

    Task<ArchivoDto> EditarAsync(int id, ActualizarArchivoDto dto, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ArchivoDto>> ListarPorCarpetaAsync(int carpetaId, CancellationToken ct = default);
    Task<ArchivoDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<DescargaArchivoDto> DescargarAsync(int id, CancellationToken ct = default);
}
