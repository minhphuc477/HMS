// DataAL/Models/HmsAContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DataAL.Models
{
    public class HmsAContextFactory : IDesignTimeDbContextFactory<HmsAContext>
    {
        public HmsAContext CreateDbContext(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure DbContextOptions
            var builder = new DbContextOptionsBuilder<HmsAContext>();
            var connectionString = configuration.GetConnectionString("HMS_A");
            builder.UseSqlServer(connectionString);

            return new HmsAContext(builder.Options);
        }
    }
}
