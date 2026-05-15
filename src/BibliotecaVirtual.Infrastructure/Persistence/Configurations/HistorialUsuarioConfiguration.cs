using BibliotecaVirtual.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotecaVirtual.Infrastructure.Persistence.Configurations;

public class HistorialUsuarioConfiguration : IEntityTypeConfiguration<HistorialUsuario>
{
    public void Configure(EntityTypeBuilder<HistorialUsuario> b)
    {
        b.ToTable("HistorialUsuarios");
        b.HasKey(x => x.Id);
        b.Property(x => x.CorreoUsuario).HasMaxLength(150).IsRequired();
        b.Property(x => x.CorreoEjecutor).HasMaxLength(150).IsRequired();
        b.Property(x => x.Detalle).HasMaxLength(500).IsRequired();
        b.Property(x => x.Accion).HasConversion<int>().IsRequired();
    }
}
