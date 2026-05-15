using BibliotecaVirtual.Application.Common;
using BibliotecaVirtual.Application.Usuarios.Contracts;
using BibliotecaVirtual.Application.Usuarios.Dtos;
using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;
using BibliotecaVirtual.Domain.Exceptions;
using BibliotecaVirtual.Domain.Interfaces;
using FluentValidation;

namespace BibliotecaVirtual.Application.Usuarios.Services;

/// <summary>
/// Servicio que implementa las reglas de negocio para usuarios.
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly ILogAccionRepository _logs;
    private readonly IPasswordHasher _hasher;
    private readonly CurrentUserContext _ctx;
    private readonly IValidator<CrearUsuarioDto> _crearValidator;

    public UsuarioService(
        IUsuarioRepository usuarios,
        IUnitOfWork uow,
        ILogAccionRepository logs,
        IPasswordHasher hasher,
        CurrentUserContext ctx,
        IValidator<CrearUsuarioDto> crearValidator)
    {
        _usuarios = usuarios;
        _uow = uow;
        _logs = logs;
        _hasher = hasher;
        _ctx = ctx;
        _crearValidator = crearValidator;
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto, CancellationToken ct = default)
    {
        AsegurarPermisoAdministracion();

        var result = await _crearValidator.ValidateAsync(dto, ct);
        if (!result.IsValid)
            throw new Domain.Exceptions.ValidationException(
                result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var correoNormalizado = dto.Correo.Trim().ToLowerInvariant();
        var existente = await _usuarios.ObtenerPorCorreoAsync(correoNormalizado, ct);
        if (existente != null)
        {
            if (!existente.Activo)
                throw new ConflictException("El usuario ya existe pero se encuentra inactivo. Reactívelo desde el detalle del usuario.");
            throw new ConflictException("Ya existe un usuario con ese correo.");
        }

        var usuario = new Usuario
        {
            Correo = correoNormalizado,
            NombreCompleto = dto.NombreCompleto.Trim(),
            NombreUsuario = dto.NombreUsuario.Trim(),
            PasswordHash = _hasher.Hash(dto.Password),
            Perfil = dto.Perfil,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            PuedeGestionarArchivos = dto.Perfil == TipoPerfil.AdministracionBiblioteca
                                    || dto.Perfil == TipoPerfil.GerenteUniversidad
        };

        await _usuarios.AgregarAsync(usuario, ct);
        await RegistrarHistorialAsync(usuario, TipoAccionLog.Crear, $"Usuario creado con perfil {dto.Perfil}", ct);
        await _uow.SaveChangesAsync(ct);

        return Map(usuario);
    }

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(bool incluirInactivos, CancellationToken ct = default)
    {
        AsegurarPermisoAdministracion();
        var list = await _usuarios.ListarAsync(incluirInactivos, ct);
        return list.Select(Map).ToList();
    }

    public async Task<UsuarioDto> ObtenerAsync(string correo, CancellationToken ct = default)
    {
        AsegurarPermisoAdministracion();
        var u = await _usuarios.ObtenerPorCorreoAsync(correo.ToLowerInvariant(), ct)
            ?? throw new NotFoundException("Usuario", correo);
        return Map(u);
    }

    public async Task<UsuarioDto> InactivarAsync(string correo, string motivo, CancellationToken ct = default)
    {
        AsegurarPermisoAdministracion();
        var u = await _usuarios.ObtenerPorCorreoAsync(correo.ToLowerInvariant(), ct)
            ?? throw new NotFoundException("Usuario", correo);

        if (!u.Activo) throw new ConflictException("El usuario ya está inactivo.");
        if (u.Correo == _ctx.Correo) throw new ConflictException("No puedes inactivarte a ti mismo.");

        u.Activo = false;
        u.FechaInactivacion = DateTime.UtcNow;
        _usuarios.Actualizar(u);
        await RegistrarHistorialAsync(u, TipoAccionLog.Inactivar, $"Inactivado. Motivo: {motivo}", ct);
        await _uow.SaveChangesAsync(ct);
        return Map(u);
    }

    public async Task<UsuarioDto> ReactivarAsync(string correo, string motivo, CancellationToken ct = default)
    {
        AsegurarPermisoAdministracion();
        var u = await _usuarios.ObtenerPorCorreoAsync(correo.ToLowerInvariant(), ct)
            ?? throw new NotFoundException("Usuario", correo);

        if (u.Activo) throw new ConflictException("El usuario ya se encuentra activo.");
        u.Activo = true;
        u.FechaInactivacion = null;
        _usuarios.Actualizar(u);
        await RegistrarHistorialAsync(u, TipoAccionLog.Reactivar, $"Reactivado. Motivo: {motivo}", ct);
        await _uow.SaveChangesAsync(ct);
        return Map(u);
    }

    public async Task<UsuarioDto> ActualizarPermisosAsync(string correo, OtorgarPermisosDto dto, CancellationToken ct = default)
    {
        AsegurarPermisoAdministracion();

        var u = await _usuarios.ObtenerPorCorreoAsync(correo.ToLowerInvariant(), ct)
            ?? throw new NotFoundException("Usuario", correo);

        if (u.Perfil != TipoPerfil.AsistenteBiblioteca)
            throw new ConflictException("Sólo se pueden otorgar permisos de gestión a Asistentes de Biblioteca.");

        u.PuedeGestionarArchivos = dto.PuedeGestionarArchivos;
        _usuarios.Actualizar(u);
        await RegistrarHistorialAsync(u, TipoAccionLog.OtorgarPermiso,
            $"PuedeGestionarArchivos={dto.PuedeGestionarArchivos}", ct);
        await _uow.SaveChangesAsync(ct);
        return Map(u);
    }

    public async Task<IReadOnlyList<HistorialUsuarioDto>> HistorialAsync(string correo, CancellationToken ct = default)
    {
        AsegurarPermisoAdministracion();
        var u = await _usuarios.ObtenerPorCorreoConHistorialAsync(correo.ToLowerInvariant(), ct)
            ?? throw new NotFoundException("Usuario", correo);

        return u.Historial
            .OrderByDescending(h => h.Fecha)
            .Select(h => new HistorialUsuarioDto
            {
                Id = h.Id,
                CorreoUsuario = h.CorreoUsuario,
                Accion = h.Accion,
                Detalle = h.Detalle,
                CorreoEjecutor = h.CorreoEjecutor,
                Fecha = h.Fecha
            }).ToList();
    }

    // -----------------------------------------------------------------------------------
    private void AsegurarPermisoAdministracion()
    {
        if (!_ctx.EstaAutenticado)
            throw new ForbiddenException("Se requiere autenticación.");
        if (_ctx.Perfil is not (TipoPerfil.AdministracionBiblioteca or TipoPerfil.GerenteUniversidad))
            throw new ForbiddenException("Sólo Administración de Biblioteca o Gerente de Universidad pueden gestionar usuarios.");
    }

    private async Task RegistrarHistorialAsync(Usuario usuario, TipoAccionLog accion, string detalle, CancellationToken ct)
    {
        usuario.Historial.Add(new HistorialUsuario
        {
            CorreoUsuario = usuario.Correo,
            Accion = accion,
            Detalle = detalle,
            CorreoEjecutor = _ctx.Correo ?? "sistema",
            Fecha = DateTime.UtcNow
        });

        await _logs.AgregarAsync(new LogAccion
        {
            CorreoUsuario = _ctx.Correo ?? "sistema",
            Accion = accion,
            Entidad = nameof(Usuario),
            IdEntidad = usuario.Correo,
            Detalle = detalle,
            Fecha = DateTime.UtcNow
        }, ct);
    }

    private static UsuarioDto Map(Usuario u) => new()
    {
        Correo = u.Correo,
        NombreCompleto = u.NombreCompleto,
        NombreUsuario = u.NombreUsuario,
        Perfil = u.Perfil,
        Activo = u.Activo,
        PuedeGestionarArchivos = u.PuedeGestionarArchivos,
        FechaCreacion = u.FechaCreacion,
        FechaInactivacion = u.FechaInactivacion
    };
}
