using CatalogAPI_CHAIMA.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace IntegrationTests.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Chaîne de connexion vers la base SQL Server dédiée EXCLUSIVEMENT aux tests
        public const string TestConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=CatalogDB_Test;Trusted_Connection=True;MultipleActiveResultSets=true";
protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
            // 1. Suppression de la configuration DbContext initiale(pointant vers CatalogDB)
var descriptor = services.SingleOrDefault(
d => d.ServiceType ==
typeof(DbContextOptions<CatalogDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            // 2. Réenregistrement du DbContext vers la base réelle de test
            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseSqlServer(TestConnectionString);
            });
            // 3. Application automatique des migrations sur la baseCatalogDB_Test
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            // Crée la base CatalogDB_Test si elle n'existe pas et applique toutes les migrations
db.Database.Migrate();
        });
}
}
}
