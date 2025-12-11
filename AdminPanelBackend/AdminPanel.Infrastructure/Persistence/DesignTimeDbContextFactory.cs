using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdminPanel.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        const string connection =
            "server=localhost;port=3306;database=adminpanel;user=root;password=your_password;AllowPublicKeyRetrieval=True;sslmode=none";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

        optionsBuilder.UseMySql(connection, serverVersion);
        return new AppDbContext(optionsBuilder.Options);
    }
}

