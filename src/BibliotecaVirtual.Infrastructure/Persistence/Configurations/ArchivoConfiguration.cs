using BibliotecaVirtual.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotecaVirtual.Infrastructure.Persistence.Configurations;

public class ArchivoConfiguration : IEntityTypeConfiguration<Archivo>
{
    public void Configure(EntityTypeBuilder<Archivo> b)
    {
        b.ToTable("Archivos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
        b.Property(x => x.RutaFisica).HasMaxLength(500).IsRequired();
        b.Property(x => x.Extension).HasMaxLength(10).IsRequired();
        b.Property(x => x.CorreoCreador).HasMaxLength(150).IsRequired();
        b.Property(x => x.Tipo).HasConversion<int>().IsRequired();

        b.HasOne(x => x.Carpeta)
            .WithMany(x => x.Archivos)
            .HasForeignKey(x => x.CarpetaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
