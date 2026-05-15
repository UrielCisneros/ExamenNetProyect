using BibliotecaVirtual.Application.Archivos.Configuration;
using BibliotecaVirtual.Application.Archivos.Contracts;
using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;
using BibliotecaVirtual.Domain.Interfaces;
using BibliotecaVirtual.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BibliotecaVirtual.Infrastructure.BackgroundServices;

/// <summary>
/// Proceso en segundo plano que cada minuto revisa los archivos eliminados lógicamente
/// y borra del disco aquellos cuya marca de eliminación supera los minutos configurados.
/// Para el ejercicio el valor por defecto es 2 minutos.
/// </summary>
public class LimpiezaArchivosService : BackgroundService
{
    private static readonly TimeSpan IntervaloChequeo = TimeSpan.FromSeconds(30);

    private readonly IServiceProvider _services;
    private readonly ILogger<LimpiezaArchivosService> _logger;
    private readonly AlmacenamientoSettings _settings;

    public LimpiezaArchivosService(
        IServiceProvider services,
        ILogger<LimpiezaArchivosService> logger,
        IOptions<AlmacenamientoSettings> opt)
    {
        _services = services;
        _logger = logger;
        _settings = opt.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "LimpiezaArchivosService iniciado (umbral = {min} minutos).",
            _settings.MinutosParaEliminacionFisica);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarLoteAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando limpieza de archivos.");
            }

            try { await Task.Delay(IntervaloChequeo, stoppingToken); }
            catch (TaskCanceledException) { /* shutdown */ }
        }
    }

    private async Task ProcesarLoteAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IAlmacenamientoArchivos>();
        var logs = scope.ServiceProvider.GetRequiredService<ILogAccionRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var limite = DateTime.UtcNow.AddMinutes(-_settings.MinutosParaEliminacionFisica);

        var pendientes = await ctx.Archivos
            .Where(a => !a.Activo && a.FechaEliminacionLogica != null && a.FechaEliminacionLogica <= limite)
            .ToListAsync(ct);

        if (pendientes.Count == 0) return;

        foreach (var archivo in pendientes)
        {
            try
            {
                if (storage.Existe(archivo.RutaFisica))
                    storage.Eliminar(archivo.RutaFisica);

                ctx.Archivos.Remove(archivo);

                await logs.AgregarAsync(new LogAccion
                {
                    CorreoUsuario = "sistema",
                    Accion = TipoAccionLog.Eliminar,
                    Entidad = nameof(Archivo),
                    IdEntidad = archivo.Id.ToString(),
                    Detalle = $"Borrado físico definitivo de '{archivo.Nombre}{archivo.Extension}'.",
                    Fecha = DateTime.UtcNow
                }, ct);

                _logger.LogInformation("Archivo {id} borrado físicamente ({ruta}).",
                    archivo.Id, archivo.RutaFisica);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo eliminar el archivo {id} en disco.", archivo.Id);
            }
        }

        await uow.SaveChangesAsync(ct);
    }
}
