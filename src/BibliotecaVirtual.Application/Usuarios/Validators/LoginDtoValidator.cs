using BibliotecaVirtual.Application.Usuarios.Dtos;
using FluentValidation;

namespace BibliotecaVirtual.Application.Usuarios.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Correo).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
