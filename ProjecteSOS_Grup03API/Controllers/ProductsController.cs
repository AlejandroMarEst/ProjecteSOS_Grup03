using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjecteSOS_Grup03API.Data;
using ProjecteSOS_Grup03API.DTOs;
using ProjecteSOS_Grup03API.Models;

namespace ProjecteSOS_Grup03API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            return Ok(product);
        }

        //[Authorize(Roles = "Employee,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductListDTO>>> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();

            if (products.Count == 0)
            {
                return NotFound("There are no products");
            }

            return Ok(products);
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPost("Add")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDTO productDTO)
        {
            Product product = new Product
            {
                Name = productDTO.Name,
                Description = productDTO.Description,
                Image = productDTO.Image,
                Price = productDTO.Price,
                Stock = productDTO.Stock
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllProducts), product);

        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            var result = _context.Products.Remove(product);

            if (result == null)
            {
                return BadRequest("Product not deleted");
            }

            await _context.SaveChangesAsync();

            return Ok($"Product {id} deleted");
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPatch("ReStock/{id}")]
        public async Task<IActionResult> ReStockProduct(int id, int stock)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            product.Stock += stock;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok($"Product {id} restocked");
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDTO)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            product.Name = productDTO.Name;
            product.Description = productDTO.Description;
            product.Image = productDTO.Image;
            product.Price = productDTO.Price;
            product.Stock = productDTO.Stock;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok($"Product {id} updated");
        }
    }
}
