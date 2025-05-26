using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjecteSOS_Grup03API.Data;
using ProjecteSOS_Grup03API.DTOs;
using ProjecteSOS_Grup03API.Models;
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
        [Authorize(Roles = "Admin,Worker")]
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
                return NotFound($"No s'han trobat productes per a la comanda amb id {orderId}.");
            }

            return Ok(productOrders);
        }

        // GET: api/OrderedProducts/orders/{orderId}/products/{productId}
        // Obtenir un ProductOrder per OrderId i ProductId (per admins/workers)
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("orders/{orderId}/products/{productId}")]
        public async Task<ActionResult<ProductOrderDetailsDTO>> GetProductOrder(int orderId, int productId)
        {
            var productOrder = await _context.ProductsOrders
                .Include(op => op.Product)
                .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

            if (productOrder == null)
            {
                return NotFound("Comanda del producte no trobada.");
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
                return NotFound("No s'han trobat productes demanats per aquest usuari.");
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
                return NotFound($"Comanda amb id {orderId} no trobada.");
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
                return NotFound("El client no s'ha trobat");

            var orderId = client.CurrentOrderId;

            if (orderId == null)
            {
                return BadRequest("No hi ha cap comanda activa");
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
                return NotFound("Comanda del producte no trobada.");
            }

            if (productOrder.Order.ClientId != userId)
            {
                return Forbid("No tens permís per veure aquest item.");
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
        [HttpPost()]
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
                return NotFound("No s'ha trobat el client");
            }

            var orderId = client.CurrentOrderId;

            if (orderId == null)
            {
                return BadRequest("No s'ha creat una ordre");
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound($"Comanda amb no trobada.");
            }

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                return NotFound($"Producte amb id {dto.ProductId} no trobat.");
            }

            if (userRole == "Client" && order.ClientId != userId)
            {
                return Forbid("Només pots afegir productes a les teves comandes.");
            }

            if (product.Stock < dto.Quantity)
            {
                return BadRequest($"No hi ha suficient stock per el producte {product.Name}. Disponible: {product.Stock}.");
            }

            if (await ProductOrderExists((int)orderId, dto.ProductId))
            {
                return Conflict($"Aquets producte ja existeix a la comanda. Utilitza PUT per actualitzar la quantitat.");
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
                return StatusCode(StatusCodes.Status500InternalServerError, "Error guardant a la base de dades.");
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
        [Authorize]
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
                return NotFound("item de la comanda no trobat.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Client" && existingProductOrder.Order.ClientId != userId)
            {
                return Forbid("No tens permís per editar l'item d'aquesta comanda.");
            }

            int quantityDifference = dto.Quantity - existingProductOrder.Quantity;
            if (existingProductOrder.Product.Stock < quantityDifference) // Si la nova quantitat és major, verificar si hi ha stock
            {
                return BadRequest($"No hi ha stock suficient per el producte {existingProductOrder.Product.Name}. Stock adicional requerit: {quantityDifference}, Disponible: {existingProductOrder.Product.Stock}");
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
                    return NotFound("L'ítem de la comanda ha estat eliminat o modificat per un altre usuari.");
                }
                else
                {
                    return Conflict("Problema de concurrència. Torna a carregar les dades i intenta-ho de nou.");
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error actualitzant la base de dades: " + ex.Message);
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
                return NotFound("Item de la comanda no trobat.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Comprovació de permisos
            if (userRole == "Client" && productOrder.Order.ClientId != userId)
            {
                return Forbid("No tens permís per eliminar l'item d'aquesta comanda.");
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
                return StatusCode(StatusCodes.Status500InternalServerError, "Error eliminant l'item de la comanda de la base de dades: " + ex.Message);
            }

            //return NoContent();

            return Ok(new { message = $"L'ítem del producte amb id {productId} de la comanda amb id {orderId} ha estat eliminat correctament." });
        }

        private async Task<bool> ProductOrderExists(int orderId, int productId)
        {
            return await _context.ProductsOrders.AnyAsync(e => e.OrderId == orderId && e.ProductId == productId);
        }
    }
}
