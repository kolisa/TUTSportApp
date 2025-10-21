using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TUTSportApp.Domain.Entities;

namespace TUTSportApp.Infrastructure.Data.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Address)
                .HasMaxLength(500);

            builder.Property(c => c.ContactEmail)
                .HasMaxLength(255);

            builder.Property(c => c.ContactPhone)
                .HasMaxLength(20);

            builder.HasIndex(c => c.RegistrationNumber)
                .IsUnique()
                .HasFilter("IsDeleted = 0");

            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}