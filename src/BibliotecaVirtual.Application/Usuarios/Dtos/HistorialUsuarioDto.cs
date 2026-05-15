using BibliotecaVirtual.Domain.Enums;

namespace BibliotecaVirtual.Application.Usuarios.Dtos;

public class HistorialUsuarioDto
{
    public int Id { get; set; }
    public string CorreoUsuario { get; set; } = string.Empty;
    public TipoAccionLog Accion { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public string CorreoEjecutor { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}
