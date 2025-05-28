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

        // GET: api/Orders
        [Authorize(Roles = "Admin,Manager,Employee")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderListDTO>>> GetAllOrders()
        {
            var orders = await _context.Orders.ToListAsync();

            return GetOrderList(orders);
        }

        // GET: api/Orders/5
        [Authorize(Roles = "Admin,Manager,Employee")]
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

        // GET: api/Orders/User
        // Get User Orders
        [Authorize]
        [HttpGet("User")]
        public async Task<ActionResult<IEnumerable<OrderListDTO>>> GetAllUserOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders.Where(o => o.ClientId == userId).ToListAsync();

            return GetOrderList(orders);
        }

        // GET: api/Orders/User/5
        // Get Details of a User Order
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

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, OrderDTO order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Validar si el client està intentant editar una order que no es seva
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

            // Validar que el clientid enb el DTO només el pugui canviar un admin/worker o si el client és el mateix
            if (order.ClientId != existingOrder.ClientId)
            {
                if (userRole == "Client") // un client no pot canviar el ClientId de la comanda
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

            // Validar SalesRepId si es canvia
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

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // Crea una ordre
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

        // Crea una ordre a partir de l'usuari connectat
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

            // Crear el nou order
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

            // Actualitza l'ordre actiu
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

        // Crea una ordre a partir de l'usuari connectat, un empleat, i l'email d'un client
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

            // Crear el nou order
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

            // Actualitza l'ordre actiu
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

        // Confirma l'ordre actual de l'usuari connectat
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

        // Confirma una ordre a partir de l'usuari connectat, un empleat, i l'email d'un client
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

        // DELETE: api/Orders/5
        // Elimina un ordre, els clients només poden eliminar les seves pròpies ordres
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

            if (order.ClientId != userId && userRole == "Client")
            {
                return Forbid(ErrorMessages.NoPermissionToDeleteOrder);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = string.Format(ErrorMessages.OrderDeleted, id) });
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

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
