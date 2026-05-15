using BibliotecaVirtual.Application.Carpetas.Dtos;
using FluentValidation;

namespace BibliotecaVirtual.Application.Carpetas.Validators;

public class CrearCarpetaDtoValidator : AbstractValidator<CrearCarpetaDto>
{
    public CrearCarpetaDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la carpeta es obligatorio.")
            .MaximumLength(120);

        RuleFor(x => x.Descripcion)
            .MaximumLength(500);

        RuleFor(x => x.CarpetaPadreId)
            .GreaterThan(0).WithMessage("Debe indicarse la carpeta padre.");
    }
}

public class RenombrarCarpetaDtoValidator : AbstractValidator<RenombrarCarpetaDto>
{
    public RenombrarCarpetaDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Descripcion).MaximumLength(500);
    }
}
