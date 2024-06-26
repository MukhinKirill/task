using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Data.Entities;

namespace Task.Connector.Data.Configurations;

public class UserRequestRightConfiguration : IEntityTypeConfiguration<UserRequestRight>
{
    public void Configure(EntityTypeBuilder<UserRequestRight> builder)
    {
        builder.HasKey(urr => new { urr.UserId, urr.RightId });
        builder.Property(urr => urr.UserId).HasColumnName("userId");
        builder.Property(urr => urr.RightId).HasColumnName("rightId");

        builder.HasOne(urr => urr.User)
            .WithMany()
            .HasForeignKey(urr => urr.UserId);

        builder.HasOne(urr => urr.Right)
            .WithMany()
            .HasForeignKey(urr => urr.RightId);
    }
}