using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProjectManagementApp.Infrastructure.Persistence;

// Design-time factory so `dotnet ef migrations add` can build the context without running the
// full Api host. The connection string here is used only to generate migrations; it is never
// used at runtime (Program.cs wires the real one via configuration/user-secrets).
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=projectmanagementapp_design;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options);
    }
}
