using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AcademicSystem.Infrastructure.Persistence
{
    /// <summary>
    /// Design-time factory for EF tools. Loads configuration from the project directory and creates ApplicationDbContext.
    /// This ensures `dotnet ef` commands can run without the API startup project being available.
    /// </summary>
    public class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Try to find appsettings.json in the current directory or parent directories.
            var basePath = Directory.GetCurrentDirectory();

            // If called from tools, the working directory may be the Infrastructure project; attempt to locate API project for config fallback.
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var connectionString = config.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("ACADSYS__CONNECTIONSTRING");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Could not find a connection string. Set 'DefaultConnection' in appsettings.json or environment variable 'ACADSYS__CONNECTIONSTRING'.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
