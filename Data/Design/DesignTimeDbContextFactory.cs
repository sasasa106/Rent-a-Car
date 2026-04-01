using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data.Design
{
    // Design-time factory for EF Core tools (migrations)
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Try environment variable first, then fall back to a sensible default for local development.
            var conn = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                       ?? "kur";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(conn, ServerVersion.AutoDetect(conn));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
