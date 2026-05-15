using BibliotecaVirtual.Application.Carpetas.Dtos;

namespace BibliotecaVirtual.Application.Carpetas.Contracts;

public interface ICarpetaService
{
    Task<IReadOnlyList<CarpetaDto>> ListarRaicesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CarpetaDto>> ListarHijasAsync(int padreId, CancellationToken ct = default);
    Task<CarpetaDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CarpetaArbolDto>> ObtenerArbolAsync(CancellationToken ct = default);

    Task<CarpetaDto> CrearAsync(CrearCarpetaDto dto, CancellationToken ct = default);
    Task<CarpetaDto> RenombrarAsync(int id, RenombrarCarpetaDto dto, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
