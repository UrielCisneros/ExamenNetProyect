using BibliotecaVirtual.Domain.Common;

namespace BibliotecaVirtual.Domain.Entities;

/// <summary>
/// Carpeta del sistema. Puede ser raíz (CarpetaPadreId = null) o subcarpeta.
/// Las raíces son de tipo banco, capacitaciones, comunicados, avisos, etc.
/// </summary>
public class Carpeta : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? CarpetaPadreId { get; set; }
    public Carpeta? CarpetaPadre { get; set; }
    public bool EsRaiz { get; set; }
    public string? Banco { get; set; } // sólo aplica a raíces dependientes de banco
    public string CorreoCreador { get; set; } = string.Empty;

    public ICollection<Carpeta> Subcarpetas { get; set; } = new List<Carpeta>();
    public ICollection<Archivo> Archivos { get; set; } = new List<Archivo>();
}
