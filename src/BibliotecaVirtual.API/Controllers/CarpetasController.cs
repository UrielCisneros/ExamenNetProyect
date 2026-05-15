using BibliotecaVirtual.Application.Carpetas.Contracts;
using BibliotecaVirtual.Application.Carpetas.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaVirtual.API.Controllers;

/// <summary>
/// Carpetas del sistema. Cualquier usuario autenticado puede navegar.
/// La gestión (crear/renombrar/eliminar) se restringe en el servicio a:
///   • Administración de Biblioteca
///   • Gerente de Universidad
///   • Asistente de Biblioteca CON permiso PuedeGestionarArchivos
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CarpetasController : ControllerBase
{
    private readonly ICarpetaService _service;
    public CarpetasController(ICarpetaService service) => _service = service;

    /// <summary>Carpetas raíz (bancos + categorías generales).</summary>
    [HttpGet("raices")]
    public async Task<ActionResult<IReadOnlyList<CarpetaDto>>> Raices(CancellationToken ct)
        => Ok(await _service.ListarRaicesAsync(ct));

    /// <summary>Hijas inmediatas de una carpeta.</summary>
    [HttpGet("{padreId:int}/hijas")]
    public async Task<ActionResult<IReadOnlyList<CarpetaDto>>> Hijas(int padreId, CancellationToken ct)
        => Ok(await _service.ListarHijasAsync(padreId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CarpetaDto>> Obtener(int id, CancellationToken ct)
        => Ok(await _service.ObtenerAsync(id, ct));

    /// <summary>Árbol completo de carpetas con conteo de archivos.</summary>
    [HttpGet("arbol")]
    public async Task<ActionResult<IReadOnlyList<CarpetaArbolDto>>> Arbol(CancellationToken ct)
        => Ok(await _service.ObtenerArbolAsync(ct));

    [HttpPost]
    public async Task<ActionResult<CarpetaDto>> Crear([FromBody] CrearCarpetaDto dto, CancellationToken ct)
    {
        var creada = await _service.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<CarpetaDto>> Renombrar(int id, [FromBody] RenombrarCarpetaDto dto, CancellationToken ct)
        => Ok(await _service.RenombrarAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _service.EliminarAsync(id, ct);
        return NoContent();
    }
}
