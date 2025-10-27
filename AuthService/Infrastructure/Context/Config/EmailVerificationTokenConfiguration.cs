using AuthService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Context.Config
{
    public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
    {
        public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
        {
            builder.ToTable("EmailVerificationToken");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .IsRequired();

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.ExpiresAt)
                .IsRequired();

            builder.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");

            builder.Property(e => e.UsedAt);

            // Relación con User
            builder.HasOne(e => e.User)
                .WithMany(u => u.EmailVerificationTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Token);
        }
    }
}

