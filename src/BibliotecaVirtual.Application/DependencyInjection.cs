using System.Reflection;
using BibliotecaVirtual.Application.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BibliotecaVirtual.Application;

/// <summary>
/// Registra las dependencias de la capa Application (servicios + validators).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<CurrentUserContext>();
        // Los servicios concretos se registrarán al implementarse en fases posteriores.
        return services;
    }
}
