using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Domain.Interfaces;

public interface ILogAccionRepository
{
    Task AgregarAsync(LogAccion log, CancellationToken ct = default);
    Task<IReadOnlyList<LogAccion>> ListarAsync(int top = 200, CancellationToken ct = default);

    Task<(IReadOnlyList<LogAccion> items, int total)> BuscarAsync(
        string? correo,
        string? entidad,
        TipoAccionLog? accion,
        DateTime? desde,
        DateTime? hasta,
        int pagina,
        int tamano,
        CancellationToken ct = default);
}
