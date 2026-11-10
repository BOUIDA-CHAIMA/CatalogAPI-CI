using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatalogAPI_CHAIMA.Models;

namespace CatalogAPI_CHAIMA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogDbController : ControllerBase
    {
        private readonly CatalogDbContext _context;

        public CatalogDbController(CatalogDbContext context)
        {
            _context = context;
        }

        // GET: api/CatalogDb/get-all
        [HttpGet("get-all")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/CatalogDb/get-by-id/5
        [HttpGet("get-by-id/{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }

        // PUT: api/CatalogDb/edit/5
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // POST: api/CatalogDb/create
        [HttpPost("create")]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // ⚠️ Doit pointer vers le NOUVEAU nom de l'action GET
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        // DELETE: api/CatalogDb/delete/5
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/CatalogDb/filter-by-price?price=1000&op=sup   (TP2 - Partie 3)
        [HttpGet("filter-by-price")]
        public async Task<ActionResult<IEnumerable<Product>>> FilterByPrice(decimal price, string op)
        {
            if (op != "sup" && op != "inf")
            {
                return BadRequest("L'opérateur doit être 'sup' ou 'inf'.");
            }

            var products = op == "sup"
                ? await _context.Products.Where(p => p.Price >= price).ToListAsync()
                : await _context.Products.Where(p => p.Price <= price).ToListAsync();

            return Ok(products);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}