using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Domain;

namespace Task.Connector.DataAccess.Configurations;

internal class UserRequestRightConfiguration : IEntityTypeConfiguration<UserRequestRight>
{
    public void Configure(EntityTypeBuilder<UserRequestRight> builder)
    {
        builder.ToTable("user_request_right", "task");

        builder.HasKey(x => x.UserId);
        builder.HasKey(x => x.RightId);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.RightId);

        builder.Property(x => x.UserId)
            .HasColumnType("varchar")
            .HasColumnName("user_id")
            .HasMaxLength(22)
            .IsRequired();

        builder.Property(x => x.RightId)
            .HasColumnName("right_id")
            .IsRequired();
    }
}
