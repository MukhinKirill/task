using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Domain;

namespace Task.Connector.DataAccess.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "task");

        builder.HasKey(x => x.Login);
        builder.HasIndex(x => x.Login);

        builder.Property(x => x.Login)
            .HasColumnType("varchar")
            .HasColumnName("login")
            .HasMaxLength(22)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasColumnType("varchar")
            .HasColumnName("last_name")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasColumnType("varchar")
            .HasColumnName("first_name")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.MiddleName)
            .HasColumnType("varchar")
            .HasColumnName("middle_name")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TelephoneNumber)
            .HasColumnType("varchar")
            .HasColumnName("telephone_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IsLead)
            .HasColumnName("is_lead")
            .IsRequired();
    }
}
