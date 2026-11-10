using CatalogAPI_CHAIMA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI_CHAIMA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private  readonly List<Product> _products = new();
        private  int _nextId = 1;
        [HttpPost("create-product")]
        public IActionResult CreateProduct(Product product)
        {
            product.Id = _nextId++;
            _products.Add(product);
            return Ok(product);
        }
        [HttpGet("get-all")]
        public IActionResult GetAllProducts()
        {
            return Ok(_products);
        }
        [HttpGet("getbyid/{id}")]
        public IActionResult GetProductById(int id)
        {
            var prod = _products.FirstOrDefault(p => p.Id == id);
            if (prod == null)
                return NotFound();
            return Ok(prod);
        }
    }
}
