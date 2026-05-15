namespace BibliotecaVirtual.Application.Usuarios.Dtos;

public class OtorgarPermisosDto
{
    /// <summary>
    /// Si true, el usuario podrá crear carpetas y archivos (válido para Asistente de Biblioteca).
    /// </summary>
    public bool PuedeGestionarArchivos { get; set; }
}

public class CambioEstadoUsuarioDto
{
    public string Motivo { get; set; } = string.Empty;
}
