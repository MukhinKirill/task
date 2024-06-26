using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Task.Connector.Entities.Configuration
{
    public class ItRoleConfiguration : IEntityTypeConfiguration<ItRole>
    {
        public void Configure(EntityTypeBuilder<ItRole> builder)
        {
            builder.ToTable("ItRole", "TestTaskSchema");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.CorporatePhoneNumber)
                .HasMaxLength(4)
                .HasColumnName("corporatePhoneNumber");
            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        }
    }
}
