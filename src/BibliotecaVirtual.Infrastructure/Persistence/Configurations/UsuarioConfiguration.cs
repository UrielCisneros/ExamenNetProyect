using BibliotecaVirtual.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotecaVirtual.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuarios");
        b.HasKey(x => x.Correo);
        b.Property(x => x.Correo).HasMaxLength(150).IsRequired();
        b.Property(x => x.NombreCompleto).HasMaxLength(150).IsRequired();
        b.Property(x => x.NombreUsuario).HasMaxLength(80).IsRequired();
        b.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        b.Property(x => x.Perfil).HasConversion<int>().IsRequired();
        b.Property(x => x.Activo).IsRequired();
        b.Property(x => x.PuedeGestionarArchivos).IsRequired();
        b.HasIndex(x => x.NombreUsuario).IsUnique();

        b.HasMany(x => x.Historial)
            .WithOne(h => h.Usuario)
            .HasForeignKey(h => h.CorreoUsuario)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
