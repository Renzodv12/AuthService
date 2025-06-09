using AuthService.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Context.Config
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            // Id con valor por defecto (GUID generado en C#)
            builder.Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()") 
                .IsRequired();

            // Campos requeridos y longitudes máximas
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.Email)  // índice único para email
                .IsUnique();

            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Salt)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.CI)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(u => u.BirthDate)
                .IsRequired();

            // Enum: guarda como string (más legible en BD)
            builder.Property(u => u.TypeAuth)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.CreateDate)
                .IsRequired()
                .HasDefaultValueSql("now()");

            builder.Property(u => u.LastModifiedDate)
                .IsRequired()
                .HasDefaultValueSql("now()");
        }
    }
}
