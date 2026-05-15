using BibliotecaVirtual.Application.Usuarios.Dtos;
using FluentValidation;

namespace BibliotecaVirtual.Application.Usuarios.Validators;

public class CrearUsuarioDtoValidator : AbstractValidator<CrearUsuarioDto>
{
    public CrearUsuarioDtoValidator()
    {
        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no es válido.")
            .MaximumLength(150);

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(150);

        RuleFor(x => x.NombreUsuario)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MinimumLength(3)
            .MaximumLength(80);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .MaximumLength(100);

        RuleFor(x => x.Perfil)
            .IsInEnum().WithMessage("Perfil no válido.");
    }
}
