using BibliotecaVirtual.Application.Common;
using BibliotecaVirtual.Application.Logs.Contracts;
using BibliotecaVirtual.Application.Logs.Dtos;
using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;
using BibliotecaVirtual.Domain.Exceptions;
using BibliotecaVirtual.Domain.Interfaces;

namespace BibliotecaVirtual.Application.Logs.Services;

public class LogService : ILogService
{
    private readonly ILogAccionRepository _logs;
    private readonly CurrentUserContext _ctx;

    public LogService(ILogAccionRepository logs, CurrentUserContext ctx)
    {
        _logs = logs;
        _ctx = ctx;
    }

    public async Task<PagedResultDto<LogAccionDto>> BuscarAsync(LogQueryDto query, CancellationToken ct = default)
    {
        if (!_ctx.EstaAutenticado)
            throw new ForbiddenException("Se requiere autenticación.");

        if (_ctx.Perfil is not (TipoPerfil.AdministracionBiblioteca or TipoPerfil.GerenteUniversidad))
            throw new ForbiddenException("Sólo Administración de Biblioteca o Gerente de Universidad pueden consultar la bitácora.");

        var pagina = Math.Max(1, query.Pagina);
        var tamano = Math.Clamp(query.Tamano, 1, 500);

        var (items, total) = await _logs.BuscarAsync(
            query.Correo, query.Entidad, query.Accion,
            query.Desde, query.Hasta, pagina, tamano, ct);

        return new PagedResultDto<LogAccionDto>
        {
            Pagina = pagina,
            Tamano = tamano,
            Total = total,
            Items = items.Select(Map).ToList()
        };
    }

    private static LogAccionDto Map(LogAccion l) => new()
    {
        Id = l.Id,
        CorreoUsuario = l.CorreoUsuario,
        Accion = l.Accion,
        Entidad = l.Entidad,
        IdEntidad = l.IdEntidad,
        Detalle = l.Detalle,
        Fecha = l.Fecha
    };
}
