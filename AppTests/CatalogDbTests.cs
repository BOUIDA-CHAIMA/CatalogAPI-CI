using CatalogAPI_CHAIMA.Controllers;
using CatalogAPI_CHAIMA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTests
{
    // TP2 - Tests unitaires de CatalogDbController avec EF Core InMemory
    // (la classe CatalogTests existe déjà pour le contrôleur du TP1, d'où le nom CatalogDbTests)
    public class CatalogDbTests
    {
        // Une base en mémoire différente pour chaque test
        private CatalogDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CatalogDbContext(options);
        }

        // ===================== Partie 2 - B.1 =====================
        [Fact]
        public async Task GetProducts_WhenDataExists_ReturnsAllProducts()
        {
            var context = GetDbContext();
            context.Products.AddRange(
                new Product { Name = "Table", Price = 100 },
                new Product { Name = "Chair", Price = 50 }
            );
            context.SaveChanges();

            var controller = new CatalogDbController(context);

            var result = await controller.GetAllProducts();

            var productsList = Assert.IsType<List<Product>>(result.Value);
            Assert.Equal(2, productsList.Count);
        }

        // ===================== Partie 2 - B.2 =====================
        [Fact]
        public async Task GetProduct_ExistingId_ReturnsProduct()
        {
            var context = GetDbContext();
            var prod = new Product { Id = 1, Name = "TV", Price = 1500 };
            context.Products.Add(prod);
            context.SaveChanges();

            var controller = new CatalogDbController(context);

            var result = await controller.GetProductById(1);

            Assert.NotNull(result.Value);
            Assert.Equal("TV", result.Value.Name);
        }

        // ===================== Partie 2 - B.3 =====================
        [Fact]
        public async Task GetProduct_NonExistingId_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new CatalogDbController(context);

            var result = await controller.GetProductById(99);

            Assert.Null(result.Value);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ===================== Partie 2 - B.4 : version [Theory] =====================
        [Theory]
        [InlineData(1, true)]   // Produit existant
        [InlineData(99, false)] // Produit inexistant
        public async Task GetProductById_ReturnsExpectedResult(int id, bool exists)
        {
            var context = GetDbContext();
            context.Products.Add(new Product { Id = 1, Name = "TV", Price = 1500 });
            await context.SaveChangesAsync();

            var controller = new CatalogDbController(context);

            var result = await controller.GetProductById(id);

            if (exists)
            {
                Assert.NotNull(result.Value);
                Assert.Equal("TV", result.Value.Name);
            }
            else
            {
                Assert.Null(result.Value);
                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        // ===================== Partie 2 - B.5 =====================
        [Fact]
        public async Task PostProduct_ValidData_CreatesProduct()
        {
            var context = GetDbContext();
            var controller = new CatalogDbController(context);

            var prod = new Product { Name = "Laptop", Price = 2000 };

            var result = await controller.CreateProduct(prod);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProduct = Assert.IsType<Product>(createdResult.Value);

            Assert.Equal("Laptop", createdProduct.Name);
            Assert.Equal(1, context.Products.Count());
        }

        // ===================== Partie 2 - B.6 =====================
        [Fact]
        public async Task EditProduct_ValidProduct_ReturnsDone()
        {
            var context = GetDbContext();
            var product = new Product { Id = 1, Name = "TV", Price = 1500 };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            context.Entry(context.Products.Local.First(p => p.Id == 1)).State = EntityState.Detached;

            var controller = new CatalogDbController(context);

            var updatedProduct = new Product { Id = 1, Name = "Smart TV", Price = 1800 };

            var result = await controller.EditProduct(1, updatedProduct);

            Assert.IsType<NoContentResult>(result);

            var productInDb = await context.Products.FindAsync(1);

            Assert.NotNull(productInDb);
            Assert.Equal("Smart TV", productInDb.Name);
            Assert.Equal(1800, productInDb.Price);
        }

        [Fact]
        public async Task EditProduct_IdMismatch_ReturnsBadRequest()
        {
            var context = GetDbContext();
            var controller = new CatalogDbController(context);

            var product = new Product { Id = 2, Name = "Smart TV", Price = 1800 };

            var result = await controller.EditProduct(1, product);

            Assert.IsType<BadRequestResult>(result);
        }

        // ===================== Partie 2 - B.7 =====================
        [Theory]
        [InlineData(1, true)]   // Produit existant
        [InlineData(99, false)] // Produit inexistant
        public async Task DeleteProduct_ReturnsExpectedResult(int id, bool exists)
        {
            var context = GetDbContext();
            if (exists)
            {
                context.Products.Add(new Product { Id = 1, Name = "TV", Price = 1500 });
                await context.SaveChangesAsync();
            }
            var controller = new CatalogDbController(context);
            var result = await controller.DeleteProduct(id);

            if (exists)
            {
                Assert.IsType<NoContentResult>(result);
                var deletedProduct = await context.Products.FindAsync(id);
                Assert.Null(deletedProduct);
            }
            else
            {
                Assert.IsType<NotFoundResult>(result);
            }
        }

        // ===================== Partie 3 : FilterByPrice =====================
        [Theory]
        [InlineData("sup", 1000, 2)] // 2 produits >= 1000
        [InlineData("inf", 1000, 3)] // 3 produits <= 1000
        [InlineData("sup", 3000, 0)] // aucun produit >= 3000 (prix au-dessus de tous)
        [InlineData("inf", 10, 0)]   // aucun produit <= 10 (prix en dessous de tous)
        public async Task FilterByPrice_ValidOperator_ReturnsExpectedProducts(string op, decimal price, int expectedCount)
        {
            var context = GetDbContext();

            context.Products.AddRange(
                new Product { Id = 1, Name = "Souris", Price = 50 },
                new Product { Id = 2, Name = "Clavier", Price = 100 },
                new Product { Id = 3, Name = "Ecran", Price = 1000 },
                new Product { Id = 4, Name = "TV", Price = 1500 }
            );

            await context.SaveChangesAsync();

            var controller = new CatalogDbController(context);

            var result = await controller.FilterByPrice(price, op);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Equal(expectedCount, products.Count);
        }

        // Opérateur invalide : seul "sup" ou "inf" est accepté, sinon BadRequest
        [Theory]
        [InlineData("egal")]
        [InlineData("SUP")]
        [InlineData(">=")]
        [InlineData("")]
        public async Task FilterByPrice_InvalidOperator_ReturnsBadRequest(string op)
        {
            var context = GetDbContext();
            context.Products.Add(new Product { Id = 1, Name = "TV", Price = 1500 });
            await context.SaveChangesAsync();

            var controller = new CatalogDbController(context);

            var result = await controller.FilterByPrice(1000, op);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("L'opérateur doit être 'sup' ou 'inf'.", badRequest.Value);
        }
    }
}
