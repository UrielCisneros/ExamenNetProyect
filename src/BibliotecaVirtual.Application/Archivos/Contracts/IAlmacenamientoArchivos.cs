namespace BibliotecaVirtual.Application.Archivos.Contracts;

/// <summary>
/// Abstrae el acceso al sistema de archivos físico.
/// </summary>
public interface IAlmacenamientoArchivos
{
    Task<string> GuardarAsync(int carpetaId, string nombreLogico, string extension, Stream contenido, CancellationToken ct = default);
    Stream Abrir(string rutaFisica);
    bool Existe(string rutaFisica);
    void Eliminar(string rutaFisica);
}
