using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Application.Archivos.Configuration;

/// <summary>
/// Parámetros de almacenamiento y reglas de validación de archivos.
/// </summary>
public class AlmacenamientoSettings
{
    public string RutaBase { get; set; } = "Storage";
    public int TamanoMaxArchivoMB { get; set; } = 25;
    public int TamanoMaxMultimediaMB { get; set; } = 200;
    public int MinutosParaEliminacionFisica { get; set; } = 2;

    /// <summary>Extensiones permitidas según el requerimiento.</summary>
    public static readonly IReadOnlyDictionary<string, TipoArchivo> ExtensionesPermitidas =
        new Dictionary<string, TipoArchivo>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"]  = TipoArchivo.Documento,
            [".docx"] = TipoArchivo.Documento,
            [".pptx"] = TipoArchivo.Documento,
            [".xlsx"] = TipoArchivo.Documento,
            [".mov"]  = TipoArchivo.Video,
            [".avi"]  = TipoArchivo.Video,
            [".mp3"]  = TipoArchivo.Audio
        };

    public static readonly IReadOnlyDictionary<string, string> ContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"]  = "application/pdf",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            [".mov"]  = "video/quicktime",
            [".avi"]  = "video/x-msvideo",
            [".mp3"]  = "audio/mpeg"
        };

    public long LimiteBytes(TipoArchivo tipo) =>
        (tipo == TipoArchivo.Video || tipo == TipoArchivo.Audio)
            ? (long)TamanoMaxMultimediaMB * 1024 * 1024
            : (long)TamanoMaxArchivoMB * 1024 * 1024;
}
