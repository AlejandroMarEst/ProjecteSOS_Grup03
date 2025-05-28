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
        private const string ProductNotFound = "Product not found";
        private const string NoProducts = "There are no products";
        private const string ProductNotDeleted = "Product not deleted";
        private const string ProductDeleted = "Product {0} deleted";
        private const string ProductRestocked = "Product {0} restocked";
        private const string ProductUpdated = "Product {0} updated";

        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductListDTO>> GetProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound(ProductNotFound);
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
                return NotFound(NoProducts);
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
                Stock = productDTO.Stock,
                Points = productDTO.Points
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
                return NotFound(ProductNotFound);
            }

            var result = _context.Products.Remove(product);

            if (result == null)
            {
                return BadRequest(ProductNotDeleted);
            }

            await _context.SaveChangesAsync();

            return Ok(string.Format(ProductDeleted, id));
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPatch("ReStock/{id}")]
        public async Task<IActionResult> ReStockProduct(int id, int stock)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound(ProductNotFound);
            }

            product.Stock += stock;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(string.Format(ProductRestocked, id));
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDTO)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound(ProductNotFound);
            }

            product.Name = productDTO.Name;
            product.Description = productDTO.Description;
            product.Image = productDTO.Image;
            product.Price = productDTO.Price;
            product.Stock = productDTO.Stock;
            product.Points = productDTO.Points;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(string.Format(ProductUpdated, id));
        }
    }
}
