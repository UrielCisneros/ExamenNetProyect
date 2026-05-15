using BibliotecaVirtual.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotecaVirtual.Infrastructure.Persistence.Configurations;

public class CarpetaConfiguration : IEntityTypeConfiguration<Carpeta>
{
    public void Configure(EntityTypeBuilder<Carpeta> b)
    {
        b.ToTable("Carpetas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(500);
        b.Property(x => x.Banco).HasMaxLength(120);
        b.Property(x => x.CorreoCreador).HasMaxLength(150).IsRequired();

        b.HasOne(x => x.CarpetaPadre)
            .WithMany(x => x.Subcarpetas)
            .HasForeignKey(x => x.CarpetaPadreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
