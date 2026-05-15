using BibliotecaVirtual.Application.Usuarios.Contracts;

namespace BibliotecaVirtual.Infrastructure.Identity;

/// <summary>
/// Implementación de hashing usando BCrypt.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return false;
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}
