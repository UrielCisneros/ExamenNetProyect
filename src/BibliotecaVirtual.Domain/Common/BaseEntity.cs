namespace BibliotecaVirtual.Domain.Common;

/// <summary>
/// Entidad base con propiedades de auditoría comunes.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }
    public bool Activo { get; set; } = true;
}
