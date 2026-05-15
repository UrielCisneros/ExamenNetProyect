using System.Security.Claims;
using BibliotecaVirtual.Application.Common;
using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.API.Middleware;

/// <summary>
/// Inyecta los datos del usuario autenticado (JWT) en <see cref="CurrentUserContext"/>.
/// </summary>
public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;
    public CurrentUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, CurrentUserContext user)
    {
        if (ctx.User?.Identity?.IsAuthenticated == true)
        {
            user.Correo = ctx.User.FindFirst(ClaimTypes.Email)?.Value
                          ?? ctx.User.FindFirst("sub")?.Value;

            var rol = ctx.User.FindFirst(ClaimTypes.Role)?.Value;
            if (Enum.TryParse<TipoPerfil>(rol, out var p))
                user.Perfil = p;

            var permiso = ctx.User.FindFirst("puedeGestionarArchivos")?.Value;
            user.PuedeGestionarArchivos = permiso == "true";
        }
        await _next(ctx);
    }
}
