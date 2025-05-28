using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjecteSOS_Grup03API.Data;
using ProjecteSOS_Grup03API.DTOs;
using ProjecteSOS_Grup03API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjecteSOS_Grup03API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDTO) // Register a new user
        {
            var user = new Client { UserName = userDTO.Email, Email = userDTO.Email, Name = userDTO.Name, PhoneNumber = userDTO.Phone };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            var roleResult = new IdentityResult();

            if (result.Succeeded)
            {
                roleResult = await _userManager.AddToRoleAsync(user, "Client");
            }
            if (result.Succeeded && roleResult.Succeeded)
            {
                return Ok("Client was registered");
            }

            return BadRequest(result.Errors);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Employee/register")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterDTO userDTO) // Register a new admin
        {
            var user = new Employee { UserName = userDTO.Email, Email = userDTO.Email, Name = userDTO.Name, PhoneNumber = userDTO.Phone, StartDate = DateOnly.FromDateTime(DateTime.Now) };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            var roleResult = new IdentityResult();

            if (result.Succeeded)
            {
                roleResult = await _userManager.AddToRoleAsync(user, "Employee");
            }
            if (result.Succeeded && roleResult.Succeeded)
            {
                return Ok("Employee was registered");
            }

            return BadRequest(result.Errors);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDTO userDTO) // Register a new admin
        {
            var user = new Employee { UserName = userDTO.Email, Email = userDTO.Email, Name = userDTO.Name, PhoneNumber = userDTO.Phone, StartDate = DateOnly.FromDateTime(DateTime.Now) };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            var roleResult = new IdentityResult();

            if (result.Succeeded)
            {
                roleResult = await _userManager.AddToRoleAsync(user, "Admin");
            }
            if (result.Succeeded && roleResult.Succeeded)
            {
                return Ok("Admin was registered");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")] // Login as an user or admin
        public async Task<IActionResult> Login([FromBody] LoginDTO userDTO)
        {
            var user = await _userManager.FindByEmailAsync(userDTO.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, userDTO.Password))
            {
                return Unauthorized("Invalid email or password");
            }

            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(user.UserName))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            }

            if (!string.IsNullOrEmpty(user.Id))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles != null && roles.Count > 0)
            {
                foreach (var rol in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }
            }

            var token = CreateToken(claims.ToArray());

            return Ok(token);
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<ActionResult<UserProfileDTO>> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("L'usuari no s'ha trobat");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            UserProfileDTO? profile = null;

            switch (userRole)
            {
                case "Client":
                    profile = await GetClientProfile(userId);
                    break;
                case "Employee" or "Admin":
                    profile = await GetEmployeeProfile(userId);
                    break;
                default:
                    return NotFound("No s'ha trobat cap rol en l'usuari");
            }

            if (profile == null)
            {
                return NotFound("No s'ha trobat el perfil");
            }

            return Ok(profile);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Profile/{id}")]
        public async Task<ActionResult<UserProfileDTO>> GetUserProfile(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("No s'ha trobat l'usuari");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            UserProfileDTO? profile = null;

            if (userRoles.Contains("Client"))
            {
                profile = await GetClientProfile(id);
            }
            else if (userRoles.Contains("Employee") || userRoles.Contains("Admin"))
            {
                profile = await GetEmployeeProfile(id);
            }
            else
            {
                return NotFound("No s'ha trobat cap rol en l'usuari");
            }

            if (profile == null)
            {
                return NotFound("No s'ha trobat el perfil");
            }

            return Ok(profile);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Profiles")]
        public async Task<ActionResult<IEnumerable<UserProfileDTO>>> GetUsersProfiles()
        {
            var users = await _context.Users.ToListAsync();

            if (users == null || !users.Any())
            {
                return NotFound("No hi ha usuaris registrats");
            }

            var profiles = new List<UserProfileListDTO>();

            foreach(var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                UserProfileDTO? profile = null;

                if (userRoles.Contains("Client"))
                {
                    profile = await GetClientProfile(user.Id);
                }
                else if (userRoles.Contains("Employee") || userRoles.Contains("Admin"))
                {
                    profile = await GetEmployeeProfile(user.Id);
                }
                else
                {
                    return NotFound("No s'ha trobat cap rol en l'usuari");
                }
                if (profile != null)
                {
                    profiles.Add(new UserProfileListDTO
                    {
                        Id = user.Id,
                        Email = profile.Email,
                        Name = profile.Name,
                        Phone = profile.Phone,
                        Points = profile.Points
                    });
                }
            }

            return Ok(profiles);
        }

        [Authorize]
        [HttpPut("Profile")]
        public async Task<IActionResult> Update(string name, string phone) //Cambiar a usar BBDD
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("L'usuari no s'ha trobat");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("L'usuari no està registrat");
            }

            user.PhoneNumber = phone;
            user.Name = name;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok("User was updated");
            }

            return BadRequest();
        }

        [Authorize]
        [HttpPatch("Profile/UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(string oldPasswd, string newPasswd)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("L'usuari no s'ha trobat");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("L'usuari no està registrat");
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPasswd, newPasswd);
            if (result.Succeeded)
            {
                return Ok("Password was updated");
            }
            
            return BadRequest("La contrasenya no és correcte");
        }

        [Authorize]
        [HttpDelete("Profile/DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("L'usuari no s'ha trobat");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("L'usuari no està registrat");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok("Your account was deleted");
            }

            return BadRequest();
        }

        private string CreateToken(Claim[] claims) // Creates the token for the current user
        {
            var jwtConfig = _configuration.GetSection("JwtSettings");
            var secretKey = jwtConfig["Key"];
            var issuer = jwtConfig["Issuer"];
            var audience = jwtConfig["Audience"];
            var expirationMinutes = int.Parse(jwtConfig["ExpirationMinutes"]);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<UserProfileDTO?> GetClientProfile(string userId)
        {
            var user = await _context.Clients.FirstOrDefaultAsync(c => c.Id == userId);

            if (user == null)
            {
                return null;
            }

            return new UserProfileDTO
            {
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                Points = user.Points,
                StartDate = null,
                IsAdmin = null,
            };
        }

        private async Task<UserProfileDTO?> GetEmployeeProfile(string userId)
        {
            var user = await _context.Employees.FirstOrDefaultAsync(c => c.Id == userId);

            if (user == null)
            {
                return null;
            }

            return new UserProfileDTO
            {
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                Points = null,
                StartDate = user.StartDate,
                IsAdmin = user.IsAdmin
            };
        }
    }
}
