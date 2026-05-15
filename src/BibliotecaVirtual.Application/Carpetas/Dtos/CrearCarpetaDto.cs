namespace BibliotecaVirtual.Application.Carpetas.Dtos;

/// <summary>
/// Datos para crear una subcarpeta dentro de una carpeta existente.
/// Las carpetas raíz se crean únicamente desde el seed (banco + categorías generales).
/// </summary>
public class CrearCarpetaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CarpetaPadreId { get; set; }
}

public class RenombrarCarpetaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
