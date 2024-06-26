using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Data.Entities;

namespace Task.Connector.Data.Configurations;

public class PasswordConfiguration : IEntityTypeConfiguration<Password>
{
    public void Configure(EntityTypeBuilder<Password> builder)
    {
        builder.HasKey(password => password.Id);
        builder.Property(password => password.Id).HasColumnName("id");
        builder.Property(password => password.UserId).IsRequired().HasMaxLength(22).HasColumnName("userId");
        builder.Property(password => password.PasswordValue).IsRequired().HasMaxLength(20).HasColumnName("password");
    }
}