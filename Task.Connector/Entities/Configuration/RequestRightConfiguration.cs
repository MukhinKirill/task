using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Task.Connector.Entities.Configuration
{
    public class RequestRightConfiguration : IEntityTypeConfiguration<RequestRight>
    {
        public void Configure(EntityTypeBuilder<RequestRight> builder)
        {
            builder.ToTable("RequestRight", "TestTaskSchema");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Name).HasColumnName("name");
        }
    }
}
