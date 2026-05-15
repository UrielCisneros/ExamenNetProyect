using BibliotecaVirtual.Application.Carpetas.Contracts;
using BibliotecaVirtual.Application.Carpetas.Dtos;
using BibliotecaVirtual.Application.Common;
using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;
using BibliotecaVirtual.Domain.Exceptions;
using BibliotecaVirtual.Domain.Interfaces;
using FluentValidation;

namespace BibliotecaVirtual.Application.Carpetas.Services;

/// <summary>
/// Servicio para la gestión de carpetas: navegación, creación de subcarpetas,
/// renombrado y eliminación lógica.
/// </summary>
public class CarpetaService : ICarpetaService
{
    private readonly ICarpetaRepository _carpetas;
    private readonly IArchivoRepository _archivos;
    private readonly ILogAccionRepository _logs;
    private readonly IUnitOfWork _uow;
    private readonly CurrentUserContext _ctx;
    private readonly IValidator<CrearCarpetaDto> _crearVal;
    private readonly IValidator<RenombrarCarpetaDto> _renVal;

    public CarpetaService(
        ICarpetaRepository carpetas,
        IArchivoRepository archivos,
        ILogAccionRepository logs,
        IUnitOfWork uow,
        CurrentUserContext ctx,
        IValidator<CrearCarpetaDto> crearVal,
        IValidator<RenombrarCarpetaDto> renVal)
    {
        _carpetas = carpetas;
        _archivos = archivos;
        _logs = logs;
        _uow = uow;
        _ctx = ctx;
        _crearVal = crearVal;
        _renVal = renVal;
    }

    // ---------------- Navegación (todos los usuarios autenticados) ----------------

    public async Task<IReadOnlyList<CarpetaDto>> ListarRaicesAsync(CancellationToken ct = default)
    {
        AsegurarAutenticado();
        var list = await _carpetas.ListarRaicesAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<CarpetaDto>> ListarHijasAsync(int padreId, CancellationToken ct = default)
    {
        AsegurarAutenticado();
        var padre = await _carpetas.ObtenerPorIdAsync(padreId, ct)
            ?? throw new NotFoundException("Carpeta", padreId);
        var list = await _carpetas.ListarPorPadreAsync(padre.Id, ct);
        return list.Select(Map).ToList();
    }

    public async Task<CarpetaDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        AsegurarAutenticado();
        var c = await _carpetas.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException("Carpeta", id);
        return Map(c);
    }

    public async Task<IReadOnlyList<CarpetaArbolDto>> ObtenerArbolAsync(CancellationToken ct = default)
    {
        AsegurarAutenticado();
        var todas = await _carpetas.ListarTodasActivasAsync(ct);
        // Las carpetas raíz tienen CarpetaPadreId = null; Dictionary no admite claves null,
        // por eso filtramos solo las que tienen padre antes de agrupar.
        var porPadre = todas
            .Where(c => c.CarpetaPadreId.HasValue)
            .GroupBy(c => c.CarpetaPadreId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // conteo de archivos activos por carpeta
        var conteoArchivos = new Dictionary<int, int>();
        foreach (var c in todas)
            conteoArchivos[c.Id] = (await _archivos.ListarPorCarpetaAsync(c.Id, ct)).Count;

        CarpetaArbolDto Construir(Carpeta c) => new()
        {
            Id = c.Id,
            Nombre = c.Nombre,
            EsRaiz = c.EsRaiz,
            Banco = c.Banco,
            CantidadArchivos = conteoArchivos.GetValueOrDefault(c.Id, 0),
            Subcarpetas = porPadre.TryGetValue(c.Id, out var hijos)
                ? hijos.OrderBy(h => h.Nombre).Select(Construir).ToList()
                : new List<CarpetaArbolDto>()
        };

        return todas.Where(c => c.EsRaiz)
            .OrderBy(c => c.Banco).ThenBy(c => c.Nombre)
            .Select(Construir).ToList();
    }

    // ---------------- Gestión (Admin/Gerente o Asistente con permiso) ----------------

    public async Task<CarpetaDto> CrearAsync(CrearCarpetaDto dto, CancellationToken ct = default)
    {
        AsegurarPuedeGestionar();

        var v = await _crearVal.ValidateAsync(dto, ct);
        if (!v.IsValid)
            throw new Domain.Exceptions.ValidationException(
                v.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var padre = await _carpetas.ObtenerPorIdAsync(dto.CarpetaPadreId, ct)
            ?? throw new NotFoundException("Carpeta padre", dto.CarpetaPadreId);

        if (await _carpetas.ExisteNombreEnPadreAsync(dto.Nombre.Trim(), padre.Id, null, ct))
            throw new ConflictException("Ya existe una carpeta con ese nombre en el mismo nivel.");

        var carpeta = new Carpeta
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            CarpetaPadreId = padre.Id,
            EsRaiz = false,
            Banco = padre.Banco, // hereda banco del ancestro
            CorreoCreador = _ctx.Correo!,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        await _carpetas.AgregarAsync(carpeta, ct);
        await _logs.AgregarAsync(new LogAccion
        {
            CorreoUsuario = _ctx.Correo!,
            Accion = TipoAccionLog.Crear,
            Entidad = nameof(Carpeta),
            Detalle = $"Carpeta '{carpeta.Nombre}' creada en padre {padre.Id}.",
            Fecha = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(carpeta);
    }

    public async Task<CarpetaDto> RenombrarAsync(int id, RenombrarCarpetaDto dto, CancellationToken ct = default)
    {
        AsegurarPuedeGestionar();

        var v = await _renVal.ValidateAsync(dto, ct);
        if (!v.IsValid)
            throw new Domain.Exceptions.ValidationException(
                v.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var c = await _carpetas.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException("Carpeta", id);

        if (c.EsRaiz)
            throw new ConflictException("Las carpetas raíz no pueden renombrarse.");

        if (await _carpetas.ExisteNombreEnPadreAsync(dto.Nombre.Trim(), c.CarpetaPadreId, c.Id, ct))
            throw new ConflictException("Ya existe una carpeta con ese nombre en el mismo nivel.");

        c.Nombre = dto.Nombre.Trim();
        c.Descripcion = dto.Descripcion?.Trim();
        c.FechaModificacion = DateTime.UtcNow;
        _carpetas.Actualizar(c);
        await _logs.AgregarAsync(new LogAccion
        {
            CorreoUsuario = _ctx.Correo!,
            Accion = TipoAccionLog.Editar,
            Entidad = nameof(Carpeta),
            IdEntidad = c.Id.ToString(),
            Detalle = $"Carpeta '{c.Nombre}' renombrada.",
            Fecha = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(c);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        AsegurarPuedeGestionar();

        var c = await _carpetas.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException("Carpeta", id);

        if (c.EsRaiz)
            throw new ConflictException("Las carpetas raíz no pueden eliminarse.");

        if (await _carpetas.TieneSubcarpetasOArchivosActivosAsync(c.Id, ct))
            throw new ConflictException("La carpeta contiene subcarpetas o archivos activos.");

        c.Activo = false;
        c.FechaModificacion = DateTime.UtcNow;
        _carpetas.Actualizar(c);
        await _logs.AgregarAsync(new LogAccion
        {
            CorreoUsuario = _ctx.Correo!,
            Accion = TipoAccionLog.Eliminar,
            Entidad = nameof(Carpeta),
            IdEntidad = c.Id.ToString(),
            Detalle = $"Carpeta '{c.Nombre}' eliminada lógicamente.",
            Fecha = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ----------------------------------------------------------------------------------
    private void AsegurarAutenticado()
    {
        if (!_ctx.EstaAutenticado) throw new ForbiddenException("Se requiere autenticación.");
    }

    private void AsegurarPuedeGestionar()
    {
        AsegurarAutenticado();

        bool autorizado = _ctx.Perfil switch
        {
            TipoPerfil.AdministracionBiblioteca => true,
            TipoPerfil.GerenteUniversidad => true,
            TipoPerfil.AsistenteBiblioteca => _ctx.PuedeGestionarArchivos,
            _ => false
        };
        if (!autorizado)
            throw new ForbiddenException("No tienes permiso para gestionar carpetas.");
    }

    private static CarpetaDto Map(Carpeta c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Descripcion = c.Descripcion,
        CarpetaPadreId = c.CarpetaPadreId,
        EsRaiz = c.EsRaiz,
        Banco = c.Banco,
        CorreoCreador = c.CorreoCreador,
        FechaCreacion = c.FechaCreacion,
        Activo = c.Activo
    };
}
