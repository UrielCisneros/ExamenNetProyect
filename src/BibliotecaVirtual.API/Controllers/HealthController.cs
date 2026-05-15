using Microsoft.AspNetCore.Mvc;

namespace BibliotecaVirtual.API.Controllers;

/// <summary>
/// Endpoint de health-check para validar que la solución compila y arranca correctamente.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        servicio = "Biblioteca Virtual",
        fecha = DateTime.UtcNow
    });
}
