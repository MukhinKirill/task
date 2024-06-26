using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Data.Entities;

namespace Task.Connector.Data.Configurations;

public class ItRoleConfiguration : IEntityTypeConfiguration<ItRole>
{
    public void Configure(EntityTypeBuilder<ItRole> builder)
    {
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id).HasColumnName("id");
        builder.Property(role => role.Name).IsRequired().HasMaxLength(100).HasColumnName("name");
        builder.Property(role => role.CorporatePhoneNumber).IsRequired().IsRequired().HasMaxLength(4)
            .HasColumnName("corporatePhoneNumber");
    }
}