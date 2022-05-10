using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Bot.Entities.ContextFactories
{
    public class AllicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        private const string APPSETTING_FILE = "appsettings.json";

        public ApplicationContext CreateDbContext(string[] args) 
        {
            DbContextOptionsBuilder<ApplicationContext> builder = new();

            ConfigurationBuilder configurationBuilder = new();

            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile(APPSETTING_FILE);

            IConfigurationRoot root = configurationBuilder.Build();

            builder.UseMySql(
                connectionString: root.GetConnectionString("DefaultConnection"),
                serverVersion: new MySqlServerVersion(root.GetSection("MySqlServerVersion").Value));

            return new ApplicationContext(builder.Options);
        }
    }
}
