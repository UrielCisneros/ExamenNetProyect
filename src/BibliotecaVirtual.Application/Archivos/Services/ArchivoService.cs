using BibliotecaVirtual.Application.Archivos.Configuration;
using BibliotecaVirtual.Application.Archivos.Contracts;
using BibliotecaVirtual.Application.Archivos.Dtos;
using BibliotecaVirtual.Application.Common;
using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;
using BibliotecaVirtual.Domain.Exceptions;
using BibliotecaVirtual.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BibliotecaVirtual.Application.Archivos.Services;

/// <summary>
/// Servicio para la gestión de archivos (documentos/videos/audios).
/// </summary>
public class ArchivoService : IArchivoService
{
    private readonly IArchivoRepository _archivos;
    private readonly ICarpetaRepository _carpetas;
    private readonly ILogAccionRepository _logs;
    private readonly IAlmacenamientoArchivos _storage;
    private readonly IUnitOfWork _uow;
    private readonly CurrentUserContext _ctx;
    private readonly AlmacenamientoSettings _settings;
    private readonly IValidator<ActualizarArchivoDto> _actVal;

    public ArchivoService(
        IArchivoRepository archivos,
        ICarpetaRepository carpetas,
        ILogAccionRepository logs,
        IAlmacenamientoArchivos storage,
        IUnitOfWork uow,
        CurrentUserContext ctx,
        IOptions<AlmacenamientoSettings> opt,
        IValidator<ActualizarArchivoDto> actVal)
    {
        _archivos = archivos;
        _carpetas = carpetas;
        _logs = logs;
        _storage = storage;
        _uow = uow;
        _ctx = ctx;
        _settings = opt.Value;
        _actVal = actVal;
    }

    public async Task<ArchivoDto> SubirAsync(
        int carpetaId, string nombre, string descripcion,
        string nombreOriginal, Stream contenido, long tamanoBytes,
        CancellationToken ct = default)
    {
        AsegurarPuedeGestionar();
        ValidarTextos(nombre, descripcion);

        var carpeta = await _carpetas.ObtenerPorIdAsync(carpetaId, ct)
            ?? throw new NotFoundException("Carpeta", carpetaId);

        var extension = Path.GetExtension(nombreOriginal)?.ToLowerInvariant() ?? string.Empty;
        if (!AlmacenamientoSettings.ExtensionesPermitidas.TryGetValue(extension, out var tipo))
            throw new ConflictException($"Extensión '{extension}' no permitida. " +
                $"Permitidas: {string.Join(", ", AlmacenamientoSettings.ExtensionesPermitidas.Keys)}.");

        var limite = _settings.LimiteBytes(tipo);
        if (tamanoBytes <= 0)
            throw new ConflictException("El archivo está vacío.");
        if (tamanoBytes > limite)
            throw new ConflictException(
                $"El archivo supera el tamaño máximo permitido ({limite / 1024 / 1024} MB).");

        var rutaFisica = await _storage.GuardarAsync(carpeta.Id, nombre.Trim(), extension, contenido, ct);

        var archivo = new Archivo
        {
            Nombre = nombre.Trim(),
            Descripcion = descripcion.Trim(),
            Extension = extension,
            RutaFisica = rutaFisica,
            TamanoBytes = tamanoBytes,
            Tipo = tipo,
            CarpetaId = carpeta.Id,
            CorreoCreador = _ctx.Correo!,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        await _archivos.AgregarAsync(archivo, ct);
        await RegistrarLogAsync(TipoAccionLog.Crear, archivo, "Archivo subido.", ct);
        await _uow.SaveChangesAsync(ct);
        return Map(archivo);
    }

    public async Task<ArchivoDto> EditarAsync(int id, ActualizarArchivoDto dto, CancellationToken ct = default)
    {
        AsegurarPuedeGestionar();

        var v = await _actVal.ValidateAsync(dto, ct);
        if (!v.IsValid)
            throw new Domain.Exceptions.ValidationException(
                v.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var a = await _archivos.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException("Archivo", id);

        if (!a.Activo)
            throw new ConflictException("No se puede editar un archivo eliminado.");

        a.Nombre = dto.Nombre.Trim();
        a.Descripcion = dto.Descripcion.Trim();
        a.FechaModificacion = DateTime.UtcNow;

        _archivos.Actualizar(a);
        await RegistrarLogAsync(TipoAccionLog.Editar, a, "Edición de nombre/descripción.", ct);
        await _uow.SaveChangesAsync(ct);
        return Map(a);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        AsegurarPuedeGestionar();

        var a = await _archivos.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException("Archivo", id);

        if (!a.Activo) throw new ConflictException("El archivo ya fue eliminado.");

        a.Activo = false;
        a.FechaEliminacionLogica = DateTime.UtcNow;
        _archivos.Actualizar(a);

        await RegistrarLogAsync(TipoAccionLog.Eliminar, a,
            $"Eliminación lógica. Borrado físico en {_settings.MinutosParaEliminacionFisica} min.", ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ArchivoDto>> ListarPorCarpetaAsync(int carpetaId, CancellationToken ct = default)
    {
        AsegurarAutenticado();
        var carpeta = await _carpetas.ObtenerPorIdAsync(carpetaId, ct)
            ?? throw new NotFoundException("Carpeta", carpetaId);
        var list = await _archivos.ListarPorCarpetaAsync(carpeta.Id, ct);
        return list.Select(Map).ToList();
    }

    public async Task<ArchivoDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        AsegurarAutenticado();
        var a = await _archivos.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException("Archivo", id);
        return Map(a);
    }

    public async Task<DescargaArchivoDto> DescargarAsync(int id, CancellationToken ct = default)
    {
        AsegurarAutenticado();

        var a = await _archivos.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException("Archivo", id);

        if (!a.Activo)
            throw new ConflictException("El archivo se encuentra eliminado.");

        if (!_storage.Existe(a.RutaFisica))
            throw new NotFoundException("Archivo físico", a.RutaFisica);

        await RegistrarLogAsync(TipoAccionLog.Descargar, a, "Descarga solicitada.", ct);
        await _uow.SaveChangesAsync(ct);

        AlmacenamientoSettings.ContentTypes.TryGetValue(a.Extension, out var ct2);
        return new DescargaArchivoDto
        {
            Contenido = _storage.Abrir(a.RutaFisica),
            NombreOriginal = a.Nombre + a.Extension,
            ContentType = ct2 ?? "application/octet-stream"
        };
    }

    // ------------------------------------------------------------------
    private void ValidarTextos(string nombre, string descripcion)
    {
        var errores = new Dictionary<string, List<string>>();
        if (string.IsNullOrWhiteSpace(nombre)) errores["nombre"] = new() { "El nombre es obligatorio." };
        else if (nombre.Length > 50) errores["nombre"] = new() { "El nombre no puede exceder 50 caracteres." };
        if (string.IsNullOrWhiteSpace(descripcion)) errores["descripcion"] = new() { "La descripción es obligatoria." };
        else if (descripcion.Length > 500) errores["descripcion"] = new() { "La descripción no puede exceder 500 caracteres." };
        if (errores.Count > 0)
            throw new Domain.Exceptions.ValidationException(
                errores.ToDictionary(k => k.Key, v => v.Value.ToArray()));
    }

    private void AsegurarAutenticado()
    {
        if (!_ctx.EstaAutenticado) throw new ForbiddenException("Se requiere autenticación.");
    }

    private void AsegurarPuedeGestionar()
    {
        AsegurarAutenticado();
        var ok = _ctx.Perfil switch
        {
            TipoPerfil.AdministracionBiblioteca => true,
            TipoPerfil.GerenteUniversidad => true,
            TipoPerfil.AsistenteBiblioteca => _ctx.PuedeGestionarArchivos,
            _ => false
        };
        if (!ok) throw new ForbiddenException("No tienes permiso para gestionar archivos.");
    }

    private Task RegistrarLogAsync(TipoAccionLog accion, Archivo a, string detalle, CancellationToken ct)
        => _logs.AgregarAsync(new LogAccion
        {
            CorreoUsuario = _ctx.Correo ?? "sistema",
            Accion = accion,
            Entidad = nameof(Archivo),
            IdEntidad = a.Id.ToString(),
            Detalle = $"{detalle} (Archivo: {a.Nombre}{a.Extension})",
            Fecha = DateTime.UtcNow
        }, ct);

    private static ArchivoDto Map(Archivo a) => new()
    {
        Id = a.Id,
        Nombre = a.Nombre,
        Descripcion = a.Descripcion,
        Extension = a.Extension,
        TamanoBytes = a.TamanoBytes,
        Tipo = a.Tipo,
        CarpetaId = a.CarpetaId,
        CorreoCreador = a.CorreoCreador,
        FechaCreacion = a.FechaCreacion,
        Activo = a.Activo,
        FechaEliminacionLogica = a.FechaEliminacionLogica
    };
}
