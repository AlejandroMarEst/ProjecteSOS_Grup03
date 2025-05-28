using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjecteSOS_Grup03API.Data;
using ProjecteSOS_Grup03API.DTOs;
using ProjecteSOS_Grup03API.Models;
using ProjecteSOS_Grup03API.Tools;
using System.Security.Claims;

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

        // GET: api/OrderedProducts/ForOrder/{orderId}
        // Obtenir tots els products d'una order específica (per admins/workers)
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("ForOrder/{orderId}")]
        public async Task<ActionResult<IEnumerable<ProductOrderDetailsDTO>>> GetProductsForOrder(int orderId)
        {
            var productOrders = await _context.ProductsOrders
                .Where(op => op.OrderId == orderId)
                .Include(op => op.Product) // incluir product per obtenir nom i preu
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

        // GET: api/OrderedProducts/orders/{orderId}/products/{productId}
        // Obtenir un ProductOrder per OrderId i ProductId (per admins/workers)
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

        // GET: api/OrderedProducts/User/All
        // Obtenir tots els ProductOrders de totes les comandes de l'usuari
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

        // GET:api/OrderedProducts/User/ForOrder/{orderId}
        // Obtenir tots els productes d'una order de l'usuari
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

            var dto = new ProductOrderDTO
            {
                OrderDate = productOrder.OrderDate,
                Quantity = productOrder.Quantity
            };

            return Ok(dto);
        }

        // GET: api/OrderedProducts/User/orders/{orderId}/products/{productId}
        // Obtenir un ProductOrder específic d'una order de l'usuari
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

            // Si no hi ha cap ordre creada es crea una
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

            // Actualitzar stock del producte
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
                // Login exception
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

            // Retornar la tuta per obtenir el recurs creat
            return CreatedAtAction(nameof(GetProductOrder), new { orderId = productOrder.OrderId, productId = productOrder.ProductId }, resultDto);
        }

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

            if (userRole == "Client" && existingProductOrder.Order.ClientId != userId)
            {
                return Forbid(ErrorMessages.NoPermissionToEdit);
            }

            int quantityDifference = dto.Quantity - existingProductOrder.Quantity;
            if (existingProductOrder.Product.Stock < quantityDifference) // Si la nova quantitat és major, verificar si hi ha stock
            {
                return BadRequest(string.Format(ErrorMessages.NotEnoughStock, existingProductOrder.Product.Name, quantityDifference, existingProductOrder.Product.Stock));
            }

            // Actualitzar stock de producte
            existingProductOrder.Product.Stock -= quantityDifference;
            _context.Entry(existingProductOrder.Product).State = EntityState.Modified;

            // Actualitzar la quantitat del ProductOrder
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

            //return NoContent();

            // Crear i retornar el DTO actualitzat
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

        [Authorize]
        [HttpPatch("Quantity/{productId}")]
        public async Task<IActionResult> PatchProductQuantity(int productId, int newQuantity)
        {
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

            var existingProductOrder = await _context.ProductsOrders
                .Include(po => po.Order)
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.OrderId == orderId && po.ProductId == productId);

            if (existingProductOrder == null)
            {
                return NotFound(ErrorMessages.ProductOrderNotFound);
            }

            int quantityDifference = newQuantity - existingProductOrder.Quantity;
            if (existingProductOrder.Product.Stock < quantityDifference) // Si la nova quantitat és major, verificar si hi ha stock
            {
                return BadRequest(string.Format(ErrorMessages.NotEnoughStock, existingProductOrder.Product.Name, quantityDifference, existingProductOrder.Product.Stock));
            }

            var oldProductOrderPrice = existingProductOrder.Product.Price * existingProductOrder.Quantity;
            var newProductOrderPrice = existingProductOrder.Product.Price * newQuantity;

            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound(ErrorMessages.OrderNotFound);
            }

            order.Price -= oldProductOrderPrice;
            order.Price += newProductOrderPrice;
            _context.Entry(order).State = EntityState.Modified;

            // Actualitzar stock de producte
            existingProductOrder.Product.Stock -= quantityDifference;
            _context.Entry(existingProductOrder.Product).State = EntityState.Modified;

            // Actualitzar la quantitat del ProductOrder
            existingProductOrder.Quantity = newQuantity;
            _context.Entry(existingProductOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductOrderExists((int)orderId, productId))
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

            return Ok(ErrorMessages.QuantityUpdated);
        }

        // Delete: api/OrderesProducts/orders/{orderId}/products/{productId}
        [Authorize]
        [HttpDelete("orders/{orderId}/products/{productId}")]
        public async Task<IActionResult> DeleteProductOrder(int orderId, int productId)
        {
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

            // Comprovació de permisos
            if (userRole == "Client" && productOrder.Order.ClientId != userId)
            {
                return Forbid(ErrorMessages.NoPermissionToDelete);
            }

            // Restaurar stock del producte
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
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.ErrorDeletingDb + ex.Message);
            }

            return Ok(new { message = string.Format(ErrorMessages.ProductOrderDeleted, productId, orderId) });
        }

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

            var productOrder = await _context.ProductsOrders
                .Include(po => po.Order)
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.OrderId == orderId && po.ProductId == productId);

            if (productOrder == null)
            {
                return NotFound(ErrorMessages.ProductOrderNotFound);
            }

            var ProductOrderPrice = productOrder.Product.Price * productOrder.Quantity;

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

            order.Price -= ProductOrderPrice;
            _context.Entry(order).State = EntityState.Modified;

            _context.ProductsOrders.Remove(productOrder);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.ErrorDeletingDb + ex.Message);
            }

            return Ok(ErrorMessages.ProductDeletedFromOrder);
        }

        private async Task<bool> ProductOrderExists(int orderId, int productId)
        {
            return await _context.ProductsOrders.AnyAsync(e => e.OrderId == orderId && e.ProductId == productId);
        }
    }
}
