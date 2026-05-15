namespace BibliotecaVirtual.Domain.Interfaces;

/// <summary>
/// Coordina la persistencia transaccional entre repositorios.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
