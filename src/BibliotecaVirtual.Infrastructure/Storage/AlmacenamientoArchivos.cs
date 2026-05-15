using BibliotecaVirtual.Application.Archivos.Configuration;
using BibliotecaVirtual.Application.Archivos.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BibliotecaVirtual.Infrastructure.Storage;

/// <summary>
/// Implementación de <see cref="IAlmacenamientoArchivos"/> sobre el sistema de archivos local.
/// La ruta base se toma de <c>Almacenamiento:RutaBase</c> (relativa al ContentRoot).
/// </summary>
public class AlmacenamientoArchivos : IAlmacenamientoArchivos
{
    private readonly string _rutaBase;

    public AlmacenamientoArchivos(IOptions<AlmacenamientoSettings> opt, IHostEnvironment env)
    {
        var ruta = opt.Value.RutaBase;
        _rutaBase = Path.IsPathRooted(ruta) ? ruta : Path.Combine(env.ContentRootPath, ruta);
        Directory.CreateDirectory(_rutaBase);
    }

    public async Task<string> GuardarAsync(int carpetaId, string nombreLogico, string extension, Stream contenido, CancellationToken ct = default)
    {
        var directorioCarpeta = Path.Combine(_rutaBase, carpetaId.ToString());
        Directory.CreateDirectory(directorioCarpeta);

        var slug = Sanitize(nombreLogico);
        var nombreFisico = $"{Guid.NewGuid():N}_{slug}{extension}";
        var rutaCompleta = Path.Combine(directorioCarpeta, nombreFisico);

        await using var fs = File.Create(rutaCompleta);
        await contenido.CopyToAsync(fs, ct);
        return rutaCompleta;
    }

    public Stream Abrir(string rutaFisica) => File.OpenRead(rutaFisica);
    public bool Existe(string rutaFisica) => File.Exists(rutaFisica);
    public void Eliminar(string rutaFisica)
    {
        if (File.Exists(rutaFisica)) File.Delete(rutaFisica);
    }

    private static string Sanitize(string nombre)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var limpio = new string(nombre.Where(c => !invalidos.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(limpio) ? "archivo" : limpio;
    }
}
