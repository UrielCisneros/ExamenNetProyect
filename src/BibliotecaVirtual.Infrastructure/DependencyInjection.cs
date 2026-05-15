using BibliotecaVirtual.Application.Archivos.Configuration;
using BibliotecaVirtual.Application.Archivos.Contracts;
using BibliotecaVirtual.Application.Archivos.Services;
using BibliotecaVirtual.Application.Carpetas.Contracts;
using BibliotecaVirtual.Application.Carpetas.Services;
using BibliotecaVirtual.Application.Logs.Contracts;
using BibliotecaVirtual.Application.Logs.Services;
using BibliotecaVirtual.Application.Usuarios.Contracts;
using BibliotecaVirtual.Application.Usuarios.Services;
using BibliotecaVirtual.Domain.Interfaces;
using BibliotecaVirtual.Infrastructure.BackgroundServices;
using BibliotecaVirtual.Infrastructure.Identity;
using BibliotecaVirtual.Infrastructure.Persistence;
using BibliotecaVirtual.Infrastructure.Repositories;
using BibliotecaVirtual.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BibliotecaVirtual.Infrastructure;

/// <summary>
/// Registro de servicios de infraestructura (EF Core + SQLite + repositorios).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=biblioteca.db";

        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICarpetaRepository, CarpetaRepository>();
        services.AddScoped<IArchivoRepository, ArchivoRepository>();
        services.AddScoped<ILogAccionRepository, LogAccionRepository>();

        // Identity / auth
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Almacenamiento
        services.Configure<AlmacenamientoSettings>(configuration.GetSection("Almacenamiento"));
        services.AddSingleton<IAlmacenamientoArchivos, AlmacenamientoArchivos>();

        // Application services que dependen de Infrastructure
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICarpetaService, CarpetaService>();
        services.AddScoped<IArchivoService, ArchivoService>();
        services.AddScoped<ILogService, LogService>();

        // Procesos en segundo plano
        services.AddHostedService<LimpiezaArchivosService>();

        return services;
    }
}
