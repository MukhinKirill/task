using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Task.Connector.Entities.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Login);

            builder.ToTable("User", "TestTaskSchema");

            builder.Property(e => e.Login)
                .HasMaxLength(22)
                .HasColumnName("login");
            builder.Property(e => e.FirstName)
                .HasMaxLength(20)
                .HasColumnName("firstName");
            builder.Property(e => e.IsLead).HasColumnName("isLead");
            builder.Property(e => e.LastName)
                .HasMaxLength(20)
                .HasColumnName("lastName");
            builder.Property(e => e.MiddleName)
                .HasMaxLength(20)
                .HasColumnName("middleName");
            builder.Property(e => e.TelephoneNumber)
                .HasMaxLength(20)
                .HasColumnName("telephoneNumber");
        }
    }
}
