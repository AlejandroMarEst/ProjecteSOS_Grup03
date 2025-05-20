using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("worker/register")]
        public async Task<IActionResult> RegisterWorker([FromBody] RegisterDTO userDTO) // Register a new admin
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
                return Ok("Worker was registered");
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
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
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
        [HttpPatch("updatePhone")]
        public async Task<IActionResult> Update(string phone) //Cambiar a usar BBDD
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            user.PhoneNumber = phone;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok("User was updated");
            }
            return BadRequest();
        }
        [Authorize]
        [HttpPatch("updatePassword")]
        public async Task<IActionResult> UpdatePassword(string oldPasswd, string newPasswd)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var result = await _userManager.ChangePasswordAsync(user, oldPasswd, newPasswd);
            if (result.Succeeded)
            {
                return Ok("Password was updated");
            }
            return BadRequest();
        }
        [Authorize]
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
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
    }
}
