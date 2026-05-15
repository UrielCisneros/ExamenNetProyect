using BibliotecaVirtual.Application.Usuarios.Contracts;
using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Infrastructure.Persistence;

/// <summary>
/// Crea un usuario administrador inicial para poder operar el sistema.
/// </summary>
public static class DbSeeder
{
    public const string AdminCorreo = "admin@biblioteca.local";
    public const string AdminPasswordPorDefecto = "Admin123!";

    public static async Task SeedAsync(AppDbContext ctx, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (!ctx.Usuarios.Any())
        {
            var admin = new Usuario
            {
                Correo = AdminCorreo,
                NombreCompleto = "Administrador General",
                NombreUsuario = "admin",
                PasswordHash = hasher.Hash(AdminPasswordPorDefecto),
                Perfil = TipoPerfil.AdministracionBiblioteca,
                Activo = true,
                PuedeGestionarArchivos = true,
                FechaCreacion = DateTime.UtcNow
            };
            admin.Historial.Add(new HistorialUsuario
            {
                CorreoUsuario = admin.Correo,
                Accion = TipoAccionLog.Crear,
                Detalle = "Usuario administrador creado por seed.",
                CorreoEjecutor = "sistema"
            });
            await ctx.Usuarios.AddAsync(admin, ct);
            await ctx.SaveChangesAsync(ct);
        }
    }
}
