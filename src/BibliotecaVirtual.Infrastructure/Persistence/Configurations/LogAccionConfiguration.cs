using BibliotecaVirtual.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotecaVirtual.Infrastructure.Persistence.Configurations;

public class LogAccionConfiguration : IEntityTypeConfiguration<LogAccion>
{
    public void Configure(EntityTypeBuilder<LogAccion> b)
    {
        b.ToTable("Logs");
        b.HasKey(x => x.Id);
        b.Property(x => x.CorreoUsuario).HasMaxLength(150).IsRequired();
        b.Property(x => x.Entidad).HasMaxLength(60).IsRequired();
        b.Property(x => x.IdEntidad).HasMaxLength(60);
        b.Property(x => x.Detalle).HasMaxLength(500).IsRequired();
        b.Property(x => x.Accion).HasConversion<int>().IsRequired();
    }
}
