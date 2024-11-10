using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Domain;

namespace Task.Connector.DataAccess.Configurations
{
    internal class SecurityConfiguration : IEntityTypeConfiguration<Security>
    {
        public void Configure(EntityTypeBuilder<Security> builder)
        {
            builder.ToTable("passwords", "task");

            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.HasIndex(x => x.UserId);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasColumnType("varchar")
                .HasColumnName("user_id")
                .HasMaxLength(22)
                .IsRequired();

            builder.Property(x => x.Password)
                .HasColumnType("varchar")
                .HasColumnName("password")
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}
