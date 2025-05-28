using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjecteSOS_Grup03API.Data;
using ProjecteSOS_Grup03API.DTOs;
using ProjecteSOS_Grup03API.Models;
using ProjecteSOS_Grup03API.Tools;

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

        /// <summary>
        /// Retrieves a specific product by its ID.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>The product details if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductListDTO>> GetProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound(ErrorMessages.ProductNotFound);
            }

            // Product found, return details
            return Ok(product);
        }

        /// <summary>
        /// Retrieves all products in the system.
        /// </summary>
        /// <returns>List of all products, or NotFound if none exist.</returns>
        //[Authorize(Roles = "Employee,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductListDTO>>> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();

            if (products.Count == 0)
            {
                return NotFound(ErrorMessages.NoProducts);
            }

            return Ok(products);
        }

        /// <summary>
        /// Adds a new product to the catalog. Only accessible by Employee or Admin roles.
        /// </summary>
        /// <param name="productDTO">DTO containing product details.</param>
        /// <returns>Status of the creation operation.</returns>
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

            // Return the location of the new product (could use GetProduct for more RESTful response)
            return CreatedAtAction(nameof(GetAllProducts), product);
        }

        /// <summary>
        /// Deletes a product by its ID. Only accessible by Employee or Admin roles.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>Status of the delete operation.</returns>
        [Authorize(Roles = "Employee,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound(ErrorMessages.ProductNotFound);
            }

            // Remove the product from the database
            var result = _context.Products.Remove(product);

            // Check if removal was successful (should always be true unless DB error)
            if (result == null)
            {
                return BadRequest(ErrorMessages.ProductNotDeleted);
            }

            await _context.SaveChangesAsync();

            return Ok(string.Format(ErrorMessages.ProductDeleted, id));
        }

        /// <summary>
        /// Increases the stock of a product. Only accessible by Employee or Admin roles.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <param name="stock">The amount to add to the current stock.</param>
        /// <returns>Status of the restock operation.</returns>
        [Authorize(Roles = "Employee,Admin")]
        [HttpPatch("ReStock/{id}")]
        public async Task<IActionResult> ReStockProduct(int id, int stock)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound(ErrorMessages.ProductNotFound);
            }

            // Increase stock by the specified amount
            product.Stock += stock;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(string.Format(ErrorMessages.ProductRestocked, id));
        }

        /// <summary>
        /// Updates all details of a product. Only accessible by Employee or Admin roles.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <param name="productDTO">DTO containing updated product details.</param>
        /// <returns>Status of the update operation.</returns>
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDTO)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return NotFound(ErrorMessages.ProductNotFound);
            }

            // Update product fields
            product.Name = productDTO.Name;
            product.Description = productDTO.Description;
            product.Image = productDTO.Image;
            product.Price = productDTO.Price;
            product.Stock = productDTO.Stock;
            product.Points = productDTO.Points;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(string.Format(ErrorMessages.ProductUpdated, id));
        }
    }
}
