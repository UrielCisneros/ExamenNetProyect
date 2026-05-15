using BibliotecaVirtual.Application.Archivos.Dtos;
using FluentValidation;

namespace BibliotecaVirtual.Application.Archivos.Validators;

public class ActualizarArchivoDtoValidator : AbstractValidator<ActualizarArchivoDto>
{
    public ActualizarArchivoDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del archivo es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}
