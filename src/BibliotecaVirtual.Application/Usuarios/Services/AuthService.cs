using BibliotecaVirtual.Application.Usuarios.Contracts;
using BibliotecaVirtual.Application.Usuarios.Dtos;
using BibliotecaVirtual.Domain.Entities;
using BibliotecaVirtual.Domain.Enums;
using BibliotecaVirtual.Domain.Exceptions;
using BibliotecaVirtual.Domain.Interfaces;
using FluentValidation;

namespace BibliotecaVirtual.Application.Usuarios.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _uow;
    private readonly ILogAccionRepository _logs;
    private readonly IValidator<LoginDto> _validator;

    public AuthService(
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IUnitOfWork uow,
        ILogAccionRepository logs,
        IValidator<LoginDto> validator)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _jwt = jwt;
        _uow = uow;
        _logs = logs;
        _validator = validator;
    }

    public async Task<LoginRespuestaDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            throw new Domain.Exceptions.ValidationException(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _usuarios.ObtenerPorCorreoAsync(correo, ct)
            ?? throw new ForbiddenException("Credenciales no válidas.");

        if (!usuario.Activo)
            throw new ForbiddenException("El usuario se encuentra inactivo.");

        if (!_hasher.Verify(dto.Password, usuario.PasswordHash))
            throw new ForbiddenException("Credenciales no válidas.");

        var (token, exp) = _jwt.GenerarToken(usuario);

        await _logs.AgregarAsync(new LogAccion
        {
            CorreoUsuario = usuario.Correo,
            Accion = TipoAccionLog.Login,
            Entidad = nameof(Usuario),
            IdEntidad = usuario.Correo,
            Detalle = "Inicio de sesión exitoso.",
            Fecha = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return new LoginRespuestaDto
        {
            Token = token,
            ExpiraEn = exp,
            Usuario = new UsuarioDto
            {
                Correo = usuario.Correo,
                NombreCompleto = usuario.NombreCompleto,
                NombreUsuario = usuario.NombreUsuario,
                Perfil = usuario.Perfil,
                Activo = usuario.Activo,
                PuedeGestionarArchivos = usuario.PuedeGestionarArchivos,
                FechaCreacion = usuario.FechaCreacion,
                FechaInactivacion = usuario.FechaInactivacion
            }
        };
    }
}
