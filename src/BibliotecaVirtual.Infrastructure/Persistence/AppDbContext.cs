using BibliotecaVirtual.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaVirtual.Infrastructure.Persistence;

/// <summary>
/// DbContext principal con todas las entidades del sistema.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<HistorialUsuario> HistorialUsuarios => Set<HistorialUsuario>();
    public DbSet<Carpeta> Carpetas => Set<Carpeta>();
    public DbSet<Archivo> Archivos => Set<Archivo>();
    public DbSet<LogAccion> Logs => Set<LogAccion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
