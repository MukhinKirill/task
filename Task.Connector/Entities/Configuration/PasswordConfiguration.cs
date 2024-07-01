using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Task.Connector.Entities.Configuration
{
    public class PasswordConfiguration : IEntityTypeConfiguration<Password>
    {
        public void Configure(EntityTypeBuilder<Password> builder)
        {
            builder.ToTable("Passwords", "TestTaskSchema");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.PasswordProperty)
                .HasMaxLength(20)
                .HasColumnName("password");
            builder.Property(e => e.UserId)
                .HasMaxLength(22)
                .HasColumnName("userId");
        }
    }
}
