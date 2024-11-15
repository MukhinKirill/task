using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Task.Connector
{
    public class AppDbContextFactory :IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=241977;Database=testDb");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
