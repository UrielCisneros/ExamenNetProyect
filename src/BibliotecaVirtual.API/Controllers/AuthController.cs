using BibliotecaVirtual.Application.Usuarios.Contracts;
using BibliotecaVirtual.Application.Usuarios.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaVirtual.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Inicia sesión y devuelve un JWT.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginRespuestaDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
        => Ok(await _auth.LoginAsync(dto, ct));
}
