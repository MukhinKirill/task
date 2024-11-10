using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.Connector.Domain;

namespace Task.Connector.DataAccess.Configurations;

internal class RequestRightConfiguration : IEntityTypeConfiguration<RequestRight>
{
    public void Configure(EntityTypeBuilder<RequestRight> builder)
    {
        builder.ToTable("request_right", "task");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("text")
            .HasColumnName("name")
            .IsRequired();
    }
}
