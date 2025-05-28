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
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all orders. Only accessible by admins and employees.
        /// </summary>
        /// <returns>List of all orders.</returns>
        // GET: api/Orders
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderListDTO>>> GetAllOrders()
        {
            var orders = await _context.Orders.ToListAsync();

            return GetOrderList(orders);
        }

        /// <summary>
        /// Gets a specific order by its ID. Only accessible by admins and employees.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>The order details.</returns>
        // GET: api/Orders/5
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(ErrorMessages.OrderNotFound);
            }

            return new OrderDTO
            {
                ClientId = order.ClientId,
                SalesRepId = order.SalesRepId,
                OrderDate = order.OrderDate,
                Price = order.Price
            };
        }

        /// <summary>
        /// Gets all orders for the currently authenticated user.
        /// </summary>
        /// <returns>List of the user's orders.</returns>
        // GET: api/Orders/User
        [Authorize]
        [HttpGet("User")]
        public async Task<ActionResult<IEnumerable<OrderListDTO>>> GetAllUserOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders.Where(o => o.ClientId == userId).ToListAsync();

            return GetOrderList(orders);
        }

        /// <summary>
        /// Gets the details of a specific order for the authenticated user.
        /// Clients can only access their own orders.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>Order details.</returns>
        // GET: api/Orders/User/5
        [Authorize]
        [HttpGet("User/{id}")]
        public async Task<ActionResult<OrderDTO>> GetUserOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(ErrorMessages.OrderNotFound);
            }

            if (userRole == "Client")
            {
                if (order.ClientId != userId)
                {
                    return Unauthorized(ErrorMessages.NoPermissionToViewOrder);
                }
            }            

            return new OrderDTO
            {
                ClientId = order.ClientId,
                SalesRepId = order.SalesRepId,
                OrderDate = order.OrderDate,
                Price = order.Price
            };
        }

        /// <summary>
        /// Updates an order. Clients can only update their own orders and cannot change the owner.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <param name="order">Order data to update.</param>
        /// <returns>The updated order or an error.</returns>
        // PUT: api/Orders/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, OrderDTO order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Clients can only edit their own orders
            if (userRole == "Client")
            {
                var orderBeingEdited = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == id);

                if (orderBeingEdited == null)
                {
                    return NotFound(ErrorMessages.OrderNotFoundForEdit);
                }

                if (orderBeingEdited.ClientId != userId)
                {
                    return Forbid(ErrorMessages.NoPermissionToEditOrder);
                }
            }

            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null)
            {
                return NotFound(ErrorMessages.OrderNotFound);
            }

            // Only admins/employees can change the ClientId
            if (order.ClientId != existingOrder.ClientId)
            {
                if (userRole == "Client")
                {
                    return Forbid(ErrorMessages.NoPermissionToChangeOwner);
                }

                var newClient = await _context.Clients.FindAsync(order.ClientId);

                if (newClient == null)
                {
                    return BadRequest(ErrorMessages.InvalidNewClientId);
                }

                existingOrder.ClientId = order.ClientId;
            }

            // Validate and update SalesRepId if changed
            if (order.SalesRepId != existingOrder.SalesRepId)
            {
                if (order.SalesRepId != null)
                {
                    var salesRep = await _context.Employees.FindAsync(order.SalesRepId);

                    if (salesRep == null)
                    {
                        return BadRequest(ErrorMessages.InvalidSalesRepId);
                    }

                    existingOrder.SalesRep = salesRep;
                }
                else
                {
                    existingOrder.SalesRep = null;
                }
                existingOrder.SalesRepId = order.SalesRepId;
            }

            existingOrder.Price = order.Price;
            existingOrder.OrderDate = order.OrderDate;

            _context.Entry(existingOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound(ErrorMessages.OrderNotFound);
                }
                else
                {
                    throw;
                }
            }

            var orderToReturn = new OrderListDTO
            {
                OrderId = existingOrder.OrderId,
                ClientId = existingOrder.ClientId,
                SalesRepId = existingOrder.SalesRepId,
                OrderDate = existingOrder.OrderDate,
                Price = existingOrder.Price
            };

            return Ok(orderToReturn);
        }

        /// <summary>
        /// Creates a new order as an admin. Only accessible by admins.
        /// </summary>
        /// <param name="order">Order data.</param>
        /// <returns>The created order.</returns>
        // POST: api/Orders
        [Authorize(Roles = "Admin")]
        [HttpPost("Admin/NewOrder")]
        public async Task<ActionResult<Order>> PostOrder(OrderDTO order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = await _context.Clients.FirstOrDefaultAsync(u => u.Id == order.ClientId);

            if (client == null)
            {
                return NotFound(ErrorMessages.ClientNotFound);
            }

            // Crear el nou order
            var newOrder = new Order
            {
                Client = client,
                ClientId = order.ClientId,
                SalesRep = order.SalesRepId != null ? await _context.Employees.FirstOrDefaultAsync(u => u.Id == order.SalesRepId) : null,
                SalesRepId = order.SalesRepId,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                Price = 0
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            // Only allow one active order per client
            if (client.CurrentOrderId == null)
            {
                client.CurrentOrderId = newOrder.OrderId;
            }
            else
            {
                return Conflict(ErrorMessages.OrderAlreadyExists);
            }

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            var orderToReturn = new OrderListDTO
            {
                OrderId = newOrder.OrderId,
                ClientId = newOrder.ClientId,
                SalesRepId = newOrder.SalesRepId,
                OrderDate = newOrder.OrderDate,
                Price = newOrder.Price
            };

            return CreatedAtAction(nameof(GetOrder), new { id = newOrder.OrderId }, orderToReturn);
        }

        /// <summary>
        /// Creates a new online order for the authenticated user.
        /// </summary>
        /// <returns>Success or error message.</returns>
        [Authorize]
        [HttpPost("NewOnlineOrder")]
        public async Task<IActionResult> PostOrder()
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

            // Only allow one active order per client
            if (client.CurrentOrderId == null)
            {
                client.CurrentOrderId = newOrder.OrderId;
            }
            else
            {
                return Conflict(ErrorMessages.OrderAlreadyExists);
            }

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return Ok(ErrorMessages.OrderCreated);
        }

        /// <summary>
        /// Creates a new shop order for a client by email, assigned to the current employee.
        /// </summary>
        /// <param name="clientEmail">Client's email.</param>
        /// <returns>Success or error message.</returns>
        [Authorize]
        [HttpPost("NewShopOrder")]
        public async Task<IActionResult> PostOrder(string clientEmail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == clientEmail);
            var sales = await _context.Employees.FirstOrDefaultAsync(e => e.Id == userId);

            if (client == null)
            {
                return NotFound(ErrorMessages.ClientNotFound);
            }

            if (sales == null)
            {
                return NotFound(ErrorMessages.EmployeeNotFound);
            }

            var newOrder = new Order
            {
                Client = client,
                ClientId = client.Id,
                SalesRep = sales,
                SalesRepId = sales.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                Price = 0
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            // Only allow one active order per client
            if (client.CurrentOrderId == null)
            {
                client.CurrentOrderId = newOrder.OrderId;
            }
            else
            {
                return Conflict(ErrorMessages.OrderAlreadyExists);
            }

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return Ok(ErrorMessages.OrderCreated);
        }

        /// <summary>
        /// Confirms the current active online order for the authenticated user.
        /// </summary>
        /// <returns>Success message or error.</returns>
        [Authorize]
        [HttpPatch("ConfirmOnlineOrder")]
        public async Task<IActionResult> ConfirmOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients.FindAsync(userId);

            if (client == null)
            {
                return NotFound(ErrorMessages.ClientNotFound);
            }

            var orderId = client.CurrentOrderId;

            if (orderId == null)
            {
                return BadRequest(ErrorMessages.NoActiveOrder);
            }

            // Add product points to client
            var productsPoints = await _context.ProductsOrders
                .Where(op => op.OrderId == orderId)
                .Select(op => op.Product.Points)
                .ToListAsync();

            productsPoints.ForEach(p => client.Points += p);

            client.CurrentOrderId = null;

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return Ok(new { message = ErrorMessages.OrderConfirmed });
        }

        /// <summary>
        /// Confirms the current active shop order for a client by email, performed by an admin or employee.
        /// </summary>
        /// <param name="clientEmail">Client's email.</param>
        /// <returns>Success message or error.</returns>
        [Authorize(Roles = "Admin,Employee")]
        [HttpPatch("ConfirmShopOrder")]
        public async Task<IActionResult> ConfirmOrder(string clientEmail)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == clientEmail);
            var sales = await _context.Employees.FindAsync(userId);

            if (client == null)
            {
                return NotFound(ErrorMessages.ClientNotFound);
            }

            if (sales == null)
            {
                return NotFound(ErrorMessages.EmployeeNotFound);
            }

            var orderId = client.CurrentOrderId;

            if (orderId == null)
            {
                return BadRequest(ErrorMessages.NoActiveOrder);
            }

            // Add product points to client
            var productsPoints = await _context.ProductsOrders
                .Where(op => op.OrderId == orderId)
                .Select(op => op.Product.Points)
                .ToListAsync();

            productsPoints.ForEach(p => client.Points += p);
            client.CurrentOrderId = null;

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return Ok(new { message = ErrorMessages.OrderConfirmed });
        }

        /// <summary>
        /// Deletes an order. Clients can only delete their own orders.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>Success message or error.</returns>
        // DELETE: api/Orders/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(ErrorMessages.ClientNotFound);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Clients can only delete their own orders
            if (order.ClientId != userId && userRole == "Client")
            {
                return Forbid(ErrorMessages.NoPermissionToDeleteOrder);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = string.Format(ErrorMessages.OrderDeleted, id) });
        }

        /// <summary>
        /// Checks if an order exists by its ID.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>True if the order exists, false otherwise.</returns>
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        /// <summary>
        /// Converts a list of Order entities to a list of OrderListDTOs.
        /// </summary>
        /// <param name="orders">List of Order entities.</param>
        /// <returns>List of OrderListDTOs.</returns>
        private List<OrderListDTO> GetOrderList(IEnumerable<Order> orders)
        {
            var listOrders = new List<OrderListDTO>();

            foreach (var o in orders)
            {
                listOrders.Add(new OrderListDTO
                {
                    OrderId = o.OrderId,
                    ClientId = o.ClientId,
                    SalesRepId = o.SalesRepId,
                    OrderDate = o.OrderDate,
                    Price = o.Price
                });
            }

            return listOrders;
        }
    }
}
