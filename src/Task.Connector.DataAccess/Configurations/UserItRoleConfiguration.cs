using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Domain;

namespace Task.Connector.DataAccess.Configurations;

internal class UserItRoleConfiguration : IEntityTypeConfiguration<UserItRole>
{
    public void Configure(EntityTypeBuilder<UserItRole> builder)
    {
        builder.ToTable("user_it_role", "task");

        builder.HasKey(x => x.UserId);
        builder.HasKey(x => x.RoleId);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.RoleId);

        builder.Property(x => x.UserId)
            .HasColumnType("varchar")
            .HasColumnName("user_id")
            .HasMaxLength(22)
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .IsRequired();
    }
}
