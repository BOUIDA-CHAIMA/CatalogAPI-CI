using CatalogAPI_CHAIMA.Models;
using IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Respawn;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class CatalogIntegrationTests :
        IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;
        private DbConnection _dbConnection = default!;
        private Respawner _respawner = default!;

        public CatalogIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(); // Crée le client HTTP connecté au serveur de test
        }

        // Exécuté AVANT chaque test : réinitialisation des tables SQL Server
        public async Task InitializeAsync()
        {
            _dbConnection = new SqlConnection(CustomWebApplicationFactory.TestConnectionString);
            await _dbConnection.OpenAsync();

            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" },
                DbAdapter = DbAdapter.SqlServer
            });

            // Nettoyage complet des tables de CatalogDB_Test
            await _respawner.ResetAsync(_dbConnection);
        }

        // Exécuté APRÈS chaque test
        public async Task DisposeAsync()
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
        //test POST
        [Fact]
        public async Task
CreateProduct_ValidPayload_InsertsInSqlServerAndReturnsCreated()
        {
            // Arrange
            var newProduct = new Product
            {
                Name = "Clavier Mecanique",
                Price = 129.500m
            };
            // Act : Appel POST HTTP réel
            var response = await _client.PostAsJsonAsync("/api/CatalogDb/create", newProduct);
            // Assert 1 : Vérification de la réponse HTTP
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            var createdProduct = await
            response.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(createdProduct);
            Assert.True(createdProduct.Id > 0, "L'Id doit être généré par SQL Server.");
            Assert.Equal("Clavier Mecanique", createdProduct.Name);
            // Assert 2 : Vérification directe par GET HTTP
            var getResponse = await _client.GetAsync($"/api/CatalogDb/get-by-id/{createdProduct.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var fetchedProduct = await
            getResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(fetchedProduct);
            Assert.Equal(129.500m, fetchedProduct.Price);
        }
        //test CRUD
        [Fact]
        public async Task Product_Lifecycle_CrudFlow_Succeeds()
        {
            // 1. CREATE (POST)
            var product = new Product { Name = "Souris Pro", Price = 75.000m };
            var postResponse = await
            _client.PostAsJsonAsync("/api/CatalogDb/create", product);
            var created = await
            postResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(created);
            // 2. UPDATE (PUT)
            created.Name = "Souris Pro V2";
            created.Price = 85.000m;
            var putResponse = await
            _client.PutAsJsonAsync($"/api/CatalogDb/edit/{created.Id}", created);
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
            // 3. READ (GET by ID)
            var getResponse = await _client.GetAsync($"/api/CatalogDb/get-by-id/{created.Id}");
            var updated = await getResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(updated);
            Assert.Equal("Souris Pro V2", updated.Name);
            Assert.Equal(85.000m, updated.Price);
            // 4. DELETE
            var deleteResponse = await
            _client.DeleteAsync($"/api/CatalogDb/delete/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            // 5. VERIFY DELETION (GET by ID -> 404)
            var verifyResponse = await _client.GetAsync($"/api/CatalogDb/get-by-id/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
        }
        //test validation FLUENT API
        [Fact]
        public async Task
CreateProduct_NameExceedingMaxLength_TriggersValidationError()
        {
            // Arrange : Nom dépassant 30 caractères (ici 45 caractères)
            var invalidProduct = new Product
            {
                Name = "Ce nom de produit depasse largement trente caracteres",
                Price = 50.000m
            };
            // Act
            var response = await _client.PostAsJsonAsync("/api/CatalogDb/create",
            invalidProduct);
            // Assert : L'API ou le modèle doit rejeter la requête (HTTP 400 ou 500 selon le middleware)
            Assert.False(response.IsSuccessStatusCode,"SQL Server ou le modèle aurait dû rejeter un nom de plus de 30caractères.");
}
        //test Filtrage
        [Fact]
        public async Task
FilterByPrice_ValidPriceAndOp_TranslatesToSqlAndReturnsCorrectCount()
        {
            // Arrange : Insertion de 3 produits
            await _client.PostAsJsonAsync("/api/CatalogDb/create", new Product
            {
                Name = "Ecran 24",
                Price = 400m
            });
            await _client.PostAsJsonAsync("/api/CatalogDb/create", new Product
            {
                Name = "Ecran 27",
                Price = 700m
            });
            await _client.PostAsJsonAsync("/api/CatalogDb/create", new Product
            {
                Name = "Ecran 32",
                Price = 1200m
            });
            // Act : Filtrer les produits avec un prix supérieur ou égal à 600
            var response = await _client.GetAsync("/api/CatalogDb/filter-by-price?price=600&op=sup");
            // Assert
            response.EnsureSuccessStatusCode();
            var products = await
            response.Content.ReadFromJsonAsync<System.Collections.Generic.List<Product>>();
            Assert.NotNull(products);
            Assert.Equal(2, products.Count);
        }

        //test erreur HTTP : id de l'URL différent de l'id du corps JSON (Partie 6.3)
        [Fact]
        public async Task EditProduct_IdMismatch_Returns400BadRequest()
        {
            // Arrange : créer un produit réel en base
            var postResponse = await _client.PostAsJsonAsync("/api/CatalogDb/create",
                new Product { Name = "Casque", Price = 99.000m });
            var created = await postResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(created);

            // Act : PUT sur /edit/{id + 1} avec un corps dont l'Id vaut id
            var response = await _client.PutAsJsonAsync($"/api/CatalogDb/edit/{created.Id + 1}", created);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Le produit en base n'a pas été modifié
            var getResponse = await _client.GetAsync($"/api/CatalogDb/get-by-id/{created.Id}");
            var unchanged = await getResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(unchanged);
            Assert.Equal("Casque", unchanged.Name);
        }
    }
}
