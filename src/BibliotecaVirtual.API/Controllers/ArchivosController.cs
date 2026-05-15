using BibliotecaVirtual.Application.Archivos.Contracts;
using BibliotecaVirtual.Application.Archivos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaVirtual.API.Controllers;

/// <summary>
/// Gestión de archivos (documentos, videos, audios) almacenados en carpetas.
/// Cualquier usuario autenticado puede listar y descargar; la creación/edición/eliminación
/// se restringe en el servicio a perfiles de gestión.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ArchivosController : ControllerBase
{
    private readonly IArchivoService _service;
    public ArchivosController(IArchivoService service) => _service = service;

    /// <summary>Listar archivos activos de una carpeta.</summary>
    [HttpGet("carpeta/{carpetaId:int}")]
    public async Task<ActionResult<IReadOnlyList<ArchivoDto>>> Listar(int carpetaId, CancellationToken ct)
        => Ok(await _service.ListarPorCarpetaAsync(carpetaId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ArchivoDto>> Obtener(int id, CancellationToken ct)
        => Ok(await _service.ObtenerAsync(id, ct));

    /// <summary>
    /// Sube un archivo a la carpeta indicada. El cuerpo es multipart/form-data con:
    ///   • carpetaId (int)
    ///   • nombre (max 50)
    ///   • descripcion (max 500)
    ///   • archivo (IFormFile, extensiones permitidas: .pdf .docx .pptx .xlsx .mov .mp3 .avi)
    /// </summary>
    [HttpPost("subir")]
    [RequestSizeLimit(300_000_000)]  // 300 MB tope absoluto del request
    [RequestFormLimits(MultipartBodyLengthLimit = 300_000_000)]
    public async Task<ActionResult<ArchivoDto>> Subir(
        [FromForm] int carpetaId,
        [FromForm] string nombre,
        [FromForm] string descripcion,
        IFormFile archivo,
        CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido." });

        await using var stream = archivo.OpenReadStream();
        var creado = await _service.SubirAsync(
            carpetaId, nombre, descripcion,
            archivo.FileName, stream, archivo.Length, ct);

        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<ArchivoDto>> Editar(int id, [FromBody] ActualizarArchivoDto dto, CancellationToken ct)
        => Ok(await _service.EditarAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _service.EliminarAsync(id, ct);
        return NoContent();
    }

    /// <summary>Descarga el archivo físico. Disponible para cualquier usuario autenticado.</summary>
    [HttpGet("{id:int}/descargar")]
    public async Task<IActionResult> Descargar(int id, CancellationToken ct)
    {
        var d = await _service.DescargarAsync(id, ct);
        return File(d.Contenido, d.ContentType, d.NombreOriginal);
    }
}
