using BibliotecaVirtual.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaVirtual.Infrastructure.Persistence;

/// <summary>
/// Garantiza que las carpetas raíz generales existan.
/// Idempotente: compara por nombre, crea solo las que faltan.
/// Raíces: Capacitaciones, Comunicados, Avisos
/// </summary>
public static class CarpetaSeeder
{
    private static readonly string[] CategoriasGenerales = ["Capacitaciones", "Comunicados", "Avisos"];

    public static async Task SeedAsync(AppDbContext ctx, CancellationToken ct = default)
    {
        var yaExisten = await ctx.Carpetas
            .Where(c => c.EsRaiz)
            .Select(c => c.Nombre)
            .ToListAsync(ct);
        var existentes = new HashSet<string>(yaExisten, StringComparer.OrdinalIgnoreCase);

        var ahora = DateTime.UtcNow;
        bool hayCambios = false;

        foreach (var cat in CategoriasGenerales)
        {
            if (existentes.Contains(cat)) continue;
            await ctx.Carpetas.AddAsync(new Carpeta
            {
                Nombre = cat,
                Descripcion = $"Carpeta general de {cat.ToLower()}.",
                EsRaiz = true,
                Banco = null,
                CorreoCreador = "sistema",
                FechaCreacion = ahora,
                Activo = true
            }, ct);
            hayCambios = true;
        }

        if (hayCambios) await ctx.SaveChangesAsync(ct);
    }
}
