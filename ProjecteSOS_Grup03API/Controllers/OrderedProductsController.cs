using System.Reflection.Metadata;
using System.Security.Claims;
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
    public class OrderedProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderedProductsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all products for a specific order. Accessible by Admins and Employees.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order.</param>
        /// <returns>List of products associated with the order.</returns>
        // GET: api/OrderedProducts/ForOrder/{orderId}
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("ForOrder/{orderId}")]
        public async Task<ActionResult<IEnumerable<ProductOrderDetailsDTO>>> GetProductsForOrder(int orderId)
        {
            var productOrders = await _context.ProductsOrders
                .Where(op => op.OrderId == orderId)
                .Include(op => op.Product) // Eager load product details for DTO
                .Select(op => new ProductOrderDetailsDTO
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    ProductName = op.Product.Name,
                    Quantity = op.Quantity,
                    OrderDate = op.OrderDate,
                    UnitPrice = op.Product.Price
                })
                .ToListAsync();

            if (!productOrders.Any())
            {
                return NotFound(string.Format(ErrorMessages.NoProductsForOrder, orderId));
            }

            return Ok(productOrders);
        }

        /// <summary>
        /// Retrieves a specific product within an order. Admins and Employees only.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <returns>Details of the product in the order.</returns>
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("orders/{orderId}/products/{productId}")]
        public async Task<ActionResult<ProductOrderDetailsDTO>> GetProductOrder(int orderId, int productId)
        {
            var productOrder = await _context.ProductsOrders
                .Include(op => op.Product)
                .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

            if (productOrder == null)
            {
                return NotFound(ErrorMessages.ProductOrderNotFound);
            }

            return Ok(new ProductOrderDetailsDTO
            {
                OrderId = productOrder.OrderId,
                ProductId = productOrder.ProductId,
                ProductName = productOrder.Product.Name,
                Quantity = productOrder.Quantity,
                OrderDate = productOrder.OrderDate,
                UnitPrice = productOrder.Product.Price
            });
        }

        /// <summary>
        /// Retrieves all products from all orders placed by the current user.
        /// </summary>
        /// <returns>List of all ordered products for the user.</returns>
        // GET: api/OrderedProducts/User/All
        [Authorize]
        [HttpGet("User/All")]
        public async Task<ActionResult<IEnumerable<ProductOrderDetailsDTO>>> GetAllUserOrderedProducts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var productOrders = await _context.ProductsOrders
                .Where(op => op.Order.ClientId == userId)
                .Include(op => op.Product)
                .Select(op => new ProductOrderDetailsDTO
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    ProductName = op.Product.Name,
                    Quantity = op.Quantity,
                    OrderDate = op.OrderDate,
                    UnitPrice = op.Product.Price
                })
                .ToListAsync();

            if (!productOrders.Any())
            {
                return NotFound(ErrorMessages.NoOrderedProductsForUser);
            }

            return Ok(productOrders);
        }

        /// <summary>
        /// Retrieves all products from a specific order belonging to the current user.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <returns>List of products for the specified user order.</returns>
        // GET:api/OrderedProducts/User/ForOrder/{orderId}
        [Authorize]
        [HttpGet("User/ForOrder/{orderId}")]
        public async Task<ActionResult<IEnumerable<ProductOrderDetailsDTO>>> GetUserProductsForOrder(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orderExists = await _context.Orders.AnyAsync(o => o.OrderId == orderId && o.ClientId == userId);

            if (!orderExists)
            {
                return NotFound(string.Format(ErrorMessages.OrderNotFound, orderId));
            }

            var productOrders = await _context.ProductsOrders
                .Where(op => op.OrderId == orderId && op.Order.ClientId == userId)
                .Include(op => op.Product)
                .Select(op => new ProductOrderDetailsDTO
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    ProductName = op.Product.Name,
                    Quantity = op.Quantity,
                    OrderDate = op.OrderDate,
                    UnitPrice = op.Product.Price
                })
                .ToListAsync();

            return Ok(productOrders);
        }

        /// <summary>
        /// Retrieves all products from the current user's active (open) order.
        /// </summary>
        /// <returns>List of products in the current active order.</returns>
        [Authorize]
        [HttpGet("User/CurrentOrder")]
        public async Task<ActionResult<ProductOrderDetailsDTO>> GetUserCurrentOrderProducts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients.FindAsync(userId);

            if (client == null)
                return NotFound(ErrorMessages.ClientNotFound);

            var orderId = client.CurrentOrderId;

            if (orderId == null)
            {
                return BadRequest(ErrorMessages.NoActiveOrder);
            }

            var productOrders = await _context.ProductsOrders
                .Where(op => op.OrderId == orderId)
                .Select(op => new ProductOrderDetailsDTO
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    ProductName = op.Product.Name,
                    Quantity = op.Quantity,
                    OrderDate = op.OrderDate,
                    UnitPrice = op.Product.Price
                })
                .ToListAsync();

            return Ok(productOrders);
        }

        /// <summary>
        /// Retrieves a specific product from the current user's active order.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <returns>Order information for the specified product.</returns>
        [Authorize]
        [HttpGet("User/CurrentOrder/{productId}")]
        public async Task<ActionResult<ProductOrderDTO>> GetUserCurrentOrderProduct(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients.FindAsync(userId);

            if (client == null)
                return NotFound(ErrorMessages.ClientNotFound);

            var orderId = client.CurrentOrderId;

            if (orderId == null)
            {
                return BadRequest(ErrorMessages.NoActiveOrder);
            }

            var productOrder = await _context.ProductsOrders.FirstOrDefaultAsync(po => po.OrderId == orderId && po.ProductId == productId);

            if (productOrder == null)
            {
                return NotFound(ErrorMessages.ProductNotFoundInOrder);
            }

            // Only return essential info for this endpoint
            var dto = new ProductOrderDTO
            {
                OrderDate = productOrder.OrderDate,
                Quantity = productOrder.Quantity
            };

            return Ok(dto);
        }

        /// <summary>
        /// Retrieves a specific product from a specific user order.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <returns>Product order details for the user.</returns>
        // GET: api/OrderedProducts/User/orders/{orderId}/products/{productId}
        [Authorize]
        [HttpGet("User/orders/{orderId}/products/{productId}")]
        public async Task<ActionResult<ProductOrderDetailsDTO>> GetUserProductOrder(int orderId, int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var productOrder = await _context.ProductsOrders
                .Include(op => op.Product)
                .Include(op => op.Order)
                .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

            if (productOrder == null)
            {
                return NotFound(ErrorMessages.ProductOrderNotFound);
            }

            // Ensure the user is authorized to access this order
            if (productOrder.Order.ClientId != userId)
            {
                return Forbid(ErrorMessages.NoPermissionForItem);
            }

            return Ok(new ProductOrderDetailsDTO
            {
                OrderId = productOrder.OrderId,
                ProductId = productOrder.ProductId,
                ProductName = productOrder.Product.Name,
                Quantity = productOrder.Quantity,
                OrderDate = productOrder.OrderDate,
                UnitPrice = productOrder.Product.Price
            });
        }

        /// <summary>
        /// Adds a new product to the current user's active order. If no active order exists, one is created.
        /// </summary>
        /// <param name="dto">DTO containing product and quantity information.</param>
        /// <returns>Details of the newly added product order.</returns>
        // POST: api/OrderedProducts
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProductOrderDetailsDTO>> PostProductOrder([FromBody] ProductOrderCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var client = await _context.Clients.FindAsync(userId);

            if (client == null)
            {
                return NotFound(ErrorMessages.ClientNotFound);
            }

            var orderId = client.CurrentOrderId;

            // Create a new order if the user has no active one
            if (orderId == null)
            {
                var newOrder = new Order
                {
                    Client = client,
                    ClientId = client.Id,
                    SalesRep = null,
                    SalesRepId = null,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    Price = 0
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                client.CurrentOrderId = newOrder.OrderId;
                _context.Clients.Update(client);
                await _context.SaveChangesAsync();

                orderId = newOrder.OrderId;
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(ErrorMessages.OrderNotFound);
            }

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                return NotFound(string.Format(ErrorMessages.ProductNotFound, dto.ProductId));
            }

            // Only allow clients to add to their own orders
            if (userRole == "Client" && order.ClientId != userId)
            {
                return Forbid(ErrorMessages.OnlyAddToOwnOrders);
            }

            if (product.Stock < dto.Quantity)
            {
                return BadRequest(string.Format(ErrorMessages.NotEnoughStock, product.Name, product.Stock));
            }

            if (await ProductOrderExists((int)orderId, dto.ProductId))
            {
                return Conflict(ErrorMessages.ProductAlreadyInOrder);
            }

            var productOrder = new ProductOrder
            {
                OrderId = (int)orderId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                OrderDate = DateTime.UtcNow
            };

            // Update product stock and order price
            product.Stock -= dto.Quantity;
            order.Price += product.Price * dto.Quantity;

            _context.ProductsOrders.Add(productOrder);
            _context.Orders.Update(order);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Database error, possibly constraint violation
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.ErrorSavingToDatabase);
            }

            var resultDto = new ProductOrderDetailsDTO
            {
                OrderId = productOrder.OrderId,
                ProductId = productOrder.ProductId,
                ProductName = product.Name,
                Quantity = productOrder.Quantity,
                OrderDate = productOrder.OrderDate,
                UnitPrice = product.Price
            };

            // Return the URI of the created resource
            return CreatedAtAction(nameof(GetProductOrder), new { orderId = productOrder.OrderId, productId = productOrder.ProductId }, resultDto);
        }

        /// <summary>
        /// Updates the quantity of a product in an order. Only Admins and Employees can perform this action.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <param name="dto">DTO with the new quantity.</param>
        /// <returns>The updated product order details.</returns>
        // PUT: api/OrderedProducts/orders/{orderId}/products/{productId}
        [Authorize (Roles = "Admin,Employee")]
        [HttpPut("orders/{orderId}/products/{productId}")]
        public async Task<IActionResult> PutProductOrder(int orderId, int productId, [FromBody] ProductOrderUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProductOrder = await _context.ProductsOrders
                .Include(po => po.Order)
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.OrderId == orderId && po.ProductId == productId);

            if (existingProductOrder == null)
            {
                return NotFound(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Only allow clients to edit their own orders (if ever allowed)
            if (userRole == "Client" && existingProductOrder.Order.ClientId != userId)
            {
                return Forbid(ErrorMessages.NoPermissionToEdit);
            }

            int quantityDifference = dto.Quantity - existingProductOrder.Quantity;
            if (existingProductOrder.Product.Stock < quantityDifference) // If increasing quantity, check for available stock
            {
                return BadRequest(string.Format(ErrorMessages.NotEnoughStock, existingProductOrder.Product.Name, quantityDifference, existingProductOrder.Product.Stock));
            }

            // If increasing quantity, check for available stock
            existingProductOrder.Product.Stock -= quantityDifference;
            _context.Entry(existingProductOrder.Product).State = EntityState.Modified;

            existingProductOrder.Quantity = dto.Quantity;
            _context.Entry(existingProductOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductOrderExists(orderId, productId))
                {
                    return NotFound(ErrorMessages.OrderItemDeletedOrModified);
                }
                else
                {
                    return Conflict(ErrorMessages.ConcurrencyProblem);
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.ErrorUpdatingDb + ex.Message);
            }

            var updatedDto = new ProductOrderDetailsDTO
            {
                OrderId = existingProductOrder.OrderId,
                ProductId = existingProductOrder.ProductId,
                ProductName = existingProductOrder.Product.Name,
                Quantity = existingProductOrder.Quantity,
                OrderDate = existingProductOrder.OrderDate,
                UnitPrice = existingProductOrder.Product.Price
            };

            return Ok(updatedDto);
        }

        /// <summary>
        /// Updates the quantity of a specific product in the current user's active order.
        /// </summary>
        /// <param name="productId">The product identifier to update.</param>
        /// <param name="newQuantity">The new desired quantity for the product.</param>
        /// <returns>Status of the update operation.</returns>
        [Authorize]
        [HttpPatch("Quantity/{productId}")]
        public async Task<IActionResult> PatchProductQuantity(int productId, int newQuantity)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients.FirstOrDefaultAsync(u => u.Id == userId);

            if (client == null)
            {
                return NotFound(ErrorMessages.ClientNotFound);
            }

            var orderId = client.CurrentOrderId;

            if (orderId == null)
            {
                return BadRequest(ErrorMessages.NoActiveOrder);
            }

            // Find the product order in the current active order
            var existingProductOrder = await _context.ProductsOrders
                .Include(po => po.Order)
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.OrderId == orderId && po.ProductId == productId);

            if (existingProductOrder == null)
            {
                return NotFound(ErrorMessages.ProductOrderNotFound);
            }

            int quantityDifference = newQuantity - existingProductOrder.Quantity;
            if (existingProductOrder.Product.Stock < quantityDifference) // If increasing quantity, check if there is enough stock available
            {
                return BadRequest(string.Format(ErrorMessages.NotEnoughStock, existingProductOrder.Product.Name, quantityDifference, existingProductOrder.Product.Stock));
            }

            // Calculate price adjustments for the order
            var oldProductOrderPrice = existingProductOrder.Product.Price * existingProductOrder.Quantity;
            var newProductOrderPrice = existingProductOrder.Product.Price * newQuantity;

            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound(ErrorMessages.OrderNotFound);
            }

            // Update order total price
            order.Price -= oldProductOrderPrice;
            order.Price += newProductOrderPrice;
            _context.Entry(order).State = EntityState.Modified;

            // Update product stock atomically with the rest of the changes
            existingProductOrder.Product.Stock -= quantityDifference;
            _context.Entry(existingProductOrder.Product).State = EntityState.Modified;

            // Update the quantity in the product order
            existingProductOrder.Quantity = newQuantity;
            _context.Entry(existingProductOrder).State = EntityState.Modified;

            // Save all changes in a single transaction to avoid race conditions
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflict (e.g., another user modified/deleted the same order item)
                if (!await ProductOrderExists((int)orderId, productId))
                {
                    return NotFound(ErrorMessages.OrderItemDeletedOrModified);
                }
                else
                {
                    // Handle other database errors (e.g., connection, constraint violations)
                    return Conflict(ErrorMessages.ConcurrencyProblem);
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.ErrorUpdatingDb + ex.Message);
            }

            return Ok(ErrorMessages.QuantityUpdated);
        }

        /// <summary>
        /// Deletes a specific product from an order. Only the owner or authorized users can perform this action.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <returns>Status of the delete operation.</returns>
        // Delete: api/OrderesProducts/orders/{orderId}/products/{productId}
        [Authorize]
        [HttpDelete("orders/{orderId}/products/{productId}")]
        public async Task<IActionResult> DeleteProductOrder(int orderId, int productId)
        {
            // Retrieve the product order, including related order and product
            var productOrder = await _context.ProductsOrders
                .Include(po => po.Order)
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.OrderId == orderId && po.ProductId == productId);

            if (productOrder == null)
            {
                return NotFound(ErrorMessages.ProductOrderNotFound);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Only allow clients to delete their own order items
            if (userRole == "Client" && productOrder.Order.ClientId != userId)
            {
                return Forbid(ErrorMessages.NoPermissionToDelete);
            }

            // Restore product stock before deleting the order item
            if (productOrder.Product != null)
            {
                productOrder.Product.Stock += productOrder.Quantity;
                _context.Entry(productOrder.Product).State = EntityState.Modified;
            }

            _context.ProductsOrders.Remove(productOrder);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Handle database errors during deletion
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.ErrorDeletingDb + ex.Message);
            }

            return Ok(new { message = string.Format(ErrorMessages.ProductOrderDeleted, productId, orderId) });
        }

        /// <summary>
        /// Deletes a specific product from the current user's active order, updating stock and order price accordingly.
        /// </summary>
        /// <param name="productId">Product identifier to delete.</param>
        /// <returns>Status of the delete operation.</returns>
        [Authorize]
        [HttpDelete("User/CurrentOrder/{productId}")]
        public async Task<IActionResult> DeleteUserCurrentOrderProduct(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients.FirstOrDefaultAsync(u => u.Id == userId);

            if (client == null)
            {
                return NotFound(ErrorMessages.ClientNotFound);
            }

            var orderId = client.CurrentOrderId;

            if (orderId == null)
            {
                return BadRequest(ErrorMessages.NoActiveOrder);
            }

            // Retrieve the product order from the user's current order
            var productOrder = await _context.ProductsOrders
                .Include(po => po.Order)
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.OrderId == orderId && po.ProductId == productId);

            if (productOrder == null)
            {
                return NotFound(ErrorMessages.ProductOrderNotFound);
            }

            var ProductOrderPrice = productOrder.Product.Price * productOrder.Quantity;

            // Restore product stock
            if (productOrder.Product != null)
            {
                productOrder.Product.Stock += productOrder.Quantity;
                _context.Entry(productOrder.Product).State = EntityState.Modified;
            }

            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound(ErrorMessages.OrderNotFound);
            }

            // Update order price after removing the product
            order.Price -= ProductOrderPrice;
            _context.Entry(order).State = EntityState.Modified;

            _context.ProductsOrders.Remove(productOrder);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Handle database errors during deletion
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.ErrorDeletingDb + ex.Message);
            }

            return Ok(ErrorMessages.ProductDeletedFromOrder);
        }

        // Helper method for checking if a ProductOrder exists
        /// <summary>
        /// Checks if a product order exists in the database.
        /// </summary>
        private async Task<bool> ProductOrderExists(int orderId, int productId)
        {
            return await _context.ProductsOrders.AnyAsync(e => e.OrderId == orderId && e.ProductId == productId);
        }
    }
}
