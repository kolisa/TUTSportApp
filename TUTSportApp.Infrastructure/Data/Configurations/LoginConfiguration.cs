using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TUTSportApp.Domain.Entities;

namespace TUTSportApp.Infrastructure.Data.Configurations
{
    public class LoginConfiguration : IEntityTypeConfiguration<Login>
    {
        public void Configure(EntityTypeBuilder<Login> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(l => l.PasswordHash)
                .IsRequired();

            builder.HasIndex(l => l.Username)
                .IsUnique()
                .HasFilter("IsDeleted = 0");

            builder.HasQueryFilter(l => !l.IsDeleted);
        }
    }
}