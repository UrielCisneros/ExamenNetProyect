using BibliotecaVirtual.Application.Usuarios.Contracts;
using BibliotecaVirtual.Application.Usuarios.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaVirtual.API.Controllers;

/// <summary>
/// Administración de usuarios. Restringido a perfiles
/// AdministracionBiblioteca y GerenteUniversidad (validado además en el servicio).
/// </summary>
[ApiController]
[Authorize(Roles = "AdministracionBiblioteca,GerenteUniversidad")]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;
    public UsuariosController(IUsuarioService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> Listar(
        [FromQuery] bool incluirInactivos = false, CancellationToken ct = default)
        => Ok(await _service.ListarAsync(incluirInactivos, ct));

    [HttpGet("{correo}")]
    public async Task<ActionResult<UsuarioDto>> Obtener(string correo, CancellationToken ct)
        => Ok(await _service.ObtenerAsync(correo, ct));

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear([FromBody] CrearUsuarioDto dto, CancellationToken ct)
    {
        var creado = await _service.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { correo = creado.Correo }, creado);
    }

    [HttpPatch("{correo}/inactivar")]
    public async Task<ActionResult<UsuarioDto>> Inactivar(string correo, [FromBody] CambioEstadoUsuarioDto dto, CancellationToken ct)
        => Ok(await _service.InactivarAsync(correo, dto.Motivo, ct));

    [HttpPatch("{correo}/reactivar")]
    public async Task<ActionResult<UsuarioDto>> Reactivar(string correo, [FromBody] CambioEstadoUsuarioDto dto, CancellationToken ct)
        => Ok(await _service.ReactivarAsync(correo, dto.Motivo, ct));

    [HttpPatch("{correo}/permisos")]
    public async Task<ActionResult<UsuarioDto>> ActualizarPermisos(string correo, [FromBody] OtorgarPermisosDto dto, CancellationToken ct)
        => Ok(await _service.ActualizarPermisosAsync(correo, dto, ct));

    [HttpGet("{correo}/historial")]
    public async Task<ActionResult<IReadOnlyList<HistorialUsuarioDto>>> Historial(string correo, CancellationToken ct)
        => Ok(await _service.HistorialAsync(correo, ct));
}
