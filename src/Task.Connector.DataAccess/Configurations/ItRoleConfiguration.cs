using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Domain;

namespace Task.Connector.DataAccess.Configurations;

internal class ItRoleConfiguration : IEntityTypeConfiguration<ItRole>
{
    public void Configure(EntityTypeBuilder<ItRole> builder)
    {
        
        builder.ToTable("it_role", "task");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("varchar")
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CorporatePhoneNumber)
            .HasColumnType("varchar")
            .HasColumnName("corporate_phone_number")
            .HasMaxLength(4)
            .IsRequired();
    }
}
