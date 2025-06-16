using AuthService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Context.Config
{
    public class UserAuthenticationMethodsConfiguration : IEntityTypeConfiguration<UserAuthenticationMethods>
    {
        public void Configure(EntityTypeBuilder<UserAuthenticationMethods> builder)
        {
            builder.ToTable("UserAuthenticationMethods");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .IsRequired();

            builder.Property(x => x.TypeAuth)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Enabled)
                .IsRequired();

            builder.Property(x => x.Key)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IdUser)
                .IsRequired();

            builder.Property(x => x.CreateDate)
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.LastModifiedDate)
                .HasDefaultValueSql("now()")
                .IsRequired();

             builder.HasOne<User>().WithMany().HasForeignKey(x => x.IdUser);
        }
    }
}
