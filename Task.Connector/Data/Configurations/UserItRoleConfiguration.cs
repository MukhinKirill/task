using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Data.Entities;

namespace Task.Connector.Data.Configurations;

public class UserItRoleConfiguration : IEntityTypeConfiguration<UserItRole>
{
    public void Configure(EntityTypeBuilder<UserItRole> builder)
    {
        builder.ToTable("UserITRole");
        
        builder.HasKey(uir => new { uir.UserId, uir.RoleId });
        builder.Property(uir => uir.UserId).HasColumnName("userId");
        builder.Property(uir => uir.RoleId).HasColumnName("roleId");

        builder.HasOne(uir => uir.User)
            .WithMany()
            .HasForeignKey(uir => uir.UserId);

        builder.HasOne(uir => uir.Role)
            .WithMany()
            .HasForeignKey(uir => uir.RoleId);
    }
}