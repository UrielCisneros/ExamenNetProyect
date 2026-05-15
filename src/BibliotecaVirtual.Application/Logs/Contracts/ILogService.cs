using BibliotecaVirtual.Application.Logs.Dtos;

namespace BibliotecaVirtual.Application.Logs.Contracts;

public interface ILogService
{
    Task<PagedResultDto<LogAccionDto>> BuscarAsync(LogQueryDto query, CancellationToken ct = default);
}
