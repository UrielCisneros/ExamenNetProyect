using BibliotecaVirtual.Domain.Entities;

namespace BibliotecaVirtual.Application.Usuarios.Contracts;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiraEn) GenerarToken(Usuario usuario);
}
