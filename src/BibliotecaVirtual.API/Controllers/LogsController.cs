using BibliotecaVirtual.Application.Logs.Contracts;
using BibliotecaVirtual.Application.Logs.Dtos;
using BibliotecaVirtual.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaVirtual.API.Controllers;

/// <summary>
/// Bitácora general del sistema. Sólo accesible para Administración de Biblioteca
/// y Gerente de Universidad.
/// </summary>
[ApiController]
[Authorize(Roles = "AdministracionBiblioteca,GerenteUniversidad")]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly ILogService _service;
    public LogsController(ILogService service) => _service = service;

    /// <summary>
    /// Consulta paginada con filtros opcionales por correo, entidad
    /// (Usuario, Carpeta, Archivo), acción y rango de fechas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<LogAccionDto>>> Buscar(
        [FromQuery] string? correo,
        [FromQuery] string? entidad,
        [FromQuery] TipoAccionLog? accion,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 50,
        CancellationToken ct = default)
    {
        var query = new LogQueryDto
        {
            Correo = correo,
            Entidad = entidad,
            Accion = accion,
            Desde = desde,
            Hasta = hasta,
            Pagina = pagina,
            Tamano = tamano
        };
        return Ok(await _service.BuscarAsync(query, ct));
    }
}
