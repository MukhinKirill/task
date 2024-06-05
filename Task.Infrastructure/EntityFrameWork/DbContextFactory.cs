using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Task.Infrastructure.EntityFrameWork;

public class DbContextFactory : IDesignTimeDbContextFactory<TaskDbContext>
{
    public TaskDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var optionsBuilder = new DbContextOptionsBuilder<TaskDbContext>()
            .UseNpgsql("User ID=postgres;Password=qwerty1234;Host=localhost;Port=5432;Database=testDb;Pooling=true;");

        return new TaskDbContext(optionsBuilder.Options);
    }
}