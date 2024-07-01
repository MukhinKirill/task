using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Task.Connector.Entities.Configuration
{
    public class UserRequestRightConfiguration : IEntityTypeConfiguration<UserRequestRight>
    {
        public void Configure(EntityTypeBuilder<UserRequestRight> builder)
        {
            builder.HasKey(e => new { e.RightId, e.UserId });

            builder.ToTable("UserRequestRight", "TestTaskSchema");

            builder.Property(e => e.RightId).HasColumnName("rightId");
            builder.Property(e => e.UserId)
                .HasMaxLength(22)
                .HasColumnName("userId");
        }
    }
}
