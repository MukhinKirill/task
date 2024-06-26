using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Data.Entities;

namespace Task.Connector.Data.Configurations;

public class RequestRightConfiguration : IEntityTypeConfiguration<RequestRight>
{
    public void Configure(EntityTypeBuilder<RequestRight> builder)
    {
        builder.HasKey(right => right.Id);
        builder.Property(right => right.Id).HasColumnName("id");
        builder.Property(right => right.Name).IsRequired().HasColumnName("name");
    }
}