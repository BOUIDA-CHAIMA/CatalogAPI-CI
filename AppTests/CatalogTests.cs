using CatalogAPI_CHAIMA.Controllers;
using CatalogAPI_CHAIMA.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTests
{
    public class CatalogTests
    {
        private readonly CatalogController _controller;
        public CatalogTests()
        {
            _controller = new CatalogController();
        }

        [Fact]
        public void CreateProduct_ValidProduct_ReturnsOkAndAssignsId()
        {
            var product = new Product
            {
                Name = "Clavier",
                Price = 80
            };
            var result = _controller.CreateProduct(product);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdProduct =
            Assert.IsType<Product>(okResult.Value);
            Assert.Equal(1, createdProduct.Id);
            Assert.Equal("Clavier", createdProduct.Name);
            Assert.Equal(80, createdProduct.Price);
        }

        [Fact]
        public void GetAllProducts_AfterCreatingProducts_ReturnsAllProducts()
        {
            _controller.CreateProduct(new Product
            {
                Name = "Clavier",
                Price = 80
            });
            _controller.CreateProduct(new Product
            {
                Name = "Souris",
                Price = 40
            });
            var result = _controller.GetAllProducts();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var products =
            Assert.IsAssignableFrom<IEnumerable<Product>>(
            okResult.Value);
            Assert.Equal(2, products.Count());
        }

        [Fact]
        public void GetProductById_ExistingId_ReturnsProduct()
        {
            var product = new Product
            {
                Name = "Ecran",
                Price = 500m
            };
            _controller.CreateProduct(product);
            var result = _controller.GetProductById(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProduct =
            Assert.IsType<Product>(okResult.Value);
            Assert.Equal(1, returnedProduct.Id);
            Assert.Equal("Ecran", returnedProduct.Name);
        }

        [Fact]
        public void GetProductById_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = _controller.GetProductById(999);
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

}
