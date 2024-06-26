using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Task.Connector.Entities.Configuration
{
    public class UserItRoleConfiguration : IEntityTypeConfiguration<UserItRole>
    {
        public void Configure(EntityTypeBuilder<UserItRole> builder)
        {
            builder.HasKey(e => new { e.RoleId, e.UserId });

            builder.ToTable("UserITRole", "TestTaskSchema");

            builder.Property(e => e.RoleId).HasColumnName("roleId");
            builder.Property(e => e.UserId)
                .HasMaxLength(22)
                .HasColumnName("userId");
        }
    }
}
