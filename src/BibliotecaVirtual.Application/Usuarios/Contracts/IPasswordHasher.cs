namespace BibliotecaVirtual.Application.Usuarios.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
