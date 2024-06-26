using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Data.Entities;

namespace Task.Connector.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Login);
        builder.Property(user => user.Login).HasMaxLength(22).HasColumnName("login");
        builder.Property(user => user.LastName).IsRequired().HasMaxLength(20).HasColumnName("lastName");
        builder.Property(user => user.FirstName).IsRequired().HasMaxLength(20).HasColumnName("firstName");
        builder.Property(user => user.MiddleName).IsRequired().HasMaxLength(20).HasColumnName("middleName");
        builder.Property(user => user.TelephoneNumber).IsRequired().HasMaxLength(20).HasColumnName("telephoneNumber");
        builder.Property(user => user.IsLead).HasColumnName("isLead");
    }
}