using BibliotecaVirtual.Application.Usuarios.Dtos;

namespace BibliotecaVirtual.Application.Usuarios.Contracts;

public interface IAuthService
{
    Task<LoginRespuestaDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
}
