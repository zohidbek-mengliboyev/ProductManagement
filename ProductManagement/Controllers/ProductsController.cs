using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Context;
using ProductManagement.Entity;
using ProductManagement.Model;

namespace ProductManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductManageDbContext _context;
        private readonly IConfiguration _configuration;

        public ProductsController(ProductManageDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var vat = decimal.Parse(_configuration["VatSettings:VAT"]);
            var products = await _context.Products.ToListAsync();
            var result = products.Select(p => new
            {
                ItemName = p.Title,
                Quantity = p.Quantity,
                Price = p.Price,
                TotalPriceWithVAT = (p.Quantity * p.Price) * (1 + vat)
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var vat = decimal.Parse(_configuration["VatSettings:VAT"]);
            var result = new
            {
                ItemName = product.Title,
                Quantity = product.Quantity,
                Price = product.Price,
                TotalPriceWithVAT = (product.Quantity * product.Price) * (1 + vat)
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Title = model.Title;
            product.Quantity = model.Quantity;
            product.Price = model.Price;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}
