using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjecteSOS_Grup03API.Data;
using ProjecteSOS_Grup03API.DTOs;
using ProjecteSOS_Grup03API.Models;
using ProjecteSOS_Grup03API.Tools;
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

        /// <summary>
        /// Registers a new client user.
        /// </summary>
        /// <param name="userDTO">User registration data.</param>
        /// <returns>Result of the registration process.</returns>
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
                return Ok(ErrorMessages.ClientRegistered);
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Registers a new employee user. Accessible only by admins.
        /// </summary>
        /// <param name="userDTO">Employee registration data.</param>
        /// <returns>Result of the registration process.</returns>
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
                return Ok(ErrorMessages.EmployeeRegistered);
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Registers a new admin user. Accessible only by admins.
        /// </summary>
        /// <param name="userDTO">Admin registration data.</param>
        /// <returns>Result of the registration process.</returns>
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
                return Ok(ErrorMessages.AdminRegistered);
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Authenticates a user or admin and returns a JWT token.
        /// </summary>
        /// <param name="userDTO">User login credentials.</param>
        /// <returns>JWT token if authentication is successful.</returns>
        [HttpPost("login")] // Login as an user or admin
        public async Task<IActionResult> Login([FromBody] LoginDTO userDTO)
        {
            var user = await _userManager.FindByEmailAsync(userDTO.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, userDTO.Password))
            {
                return Unauthorized(ErrorMessages.InvalidEmailOrPassword);
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

        /// <summary>
        /// Gets the profile of the authenticated user.
        /// </summary>
        /// <returns>User profile data.</returns>
        [Authorize]
        [HttpGet("Profile")]
        public async Task<ActionResult<UserProfileDTO>> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound(ErrorMessages.UserNotFound);
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
                    return NotFound(ErrorMessages.NoRoleFound);
            }

            if (profile == null)
            {
                return NotFound(ErrorMessages.ProfileNotFound);
            }

            return Ok(profile);
        }

        /// <summary>
        /// Gets the profile of a user by ID. Accessible only by admins.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>User profile data.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("Profile/{id}")]
        public async Task<ActionResult<UserProfileDTO>> GetUserProfile(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(ErrorMessages.UserNotFound);
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
                return NotFound(ErrorMessages.NoRoleFound);
            }

            if (profile == null)
            {
                return NotFound(ErrorMessages.ProfileNotFound);
            }

            return Ok(profile);
        }

        /// <summary>
        /// Gets the profiles of all registered users. Accessible only by admins.
        /// </summary>
        /// <returns>List of user profiles.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("Profiles")]
        public async Task<ActionResult<IEnumerable<UserProfileDTO>>> GetUsersProfiles()
        {
            var users = await _context.Users.ToListAsync();

            if (users == null || !users.Any())
            {
                return NotFound(ErrorMessages.NoRegisteredUsers);
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
                    return NotFound(ErrorMessages.NoRoleFound);
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

        /// <summary>
        /// Updates the authenticated user's name and phone number.
        /// </summary>
        /// <param name="name">New name.</param>
        /// <param name="phone">New phone number.</param>
        /// <returns>Result of the update operation.</returns>
        [Authorize]
        [HttpPut("Profile")]
        public async Task<IActionResult> Update(string name, string phone) //Cambiar a usar BBDD
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound(ErrorMessages.UserNotFound);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(ErrorMessages.UserIsNotRegistered);
            }

            user.PhoneNumber = phone;
            user.Name = name;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(ErrorMessages.UserUpdated);
            }

            return BadRequest();
        }

        /// <summary>
        /// Updates the authenticated user's password.
        /// </summary>
        /// <param name="oldPasswd">Current password.</param>
        /// <param name="newPasswd">New password.</param>
        /// <returns>Result of the password update operation.</returns>
        [Authorize]
        [HttpPatch("Profile/UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(string oldPasswd, string newPasswd)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound(ErrorMessages.UserNotFound);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(ErrorMessages.UserIsNotRegistered);
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPasswd, newPasswd);
            if (result.Succeeded)
            {
                return Ok(ErrorMessages.PasswordUpdated);
            }
            
            return BadRequest(ErrorMessages.IncorrectPassword);
        }

        /// <summary>
        /// Deletes the authenticated user's account.
        /// </summary>
        /// <returns>Result of the delete operation.</returns>
        [Authorize]
        [HttpDelete("Profile/DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound(ErrorMessages.UserNotFound);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(ErrorMessages.UserIsNotRegistered);
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(ErrorMessages.AccountDeleted);
            }

            return BadRequest();
        }

        /// <summary>
        /// Creates a JWT token for the current user with their claims.
        /// </summary>
        /// <param name="claims">User claims.</param>
        /// <returns>Generated JWT token.</returns>
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

        /// <summary>
        /// Retrieves the profile of a client user by their ID.
        /// </summary>
        /// <param name="userId">Client user ID.</param>
        /// <returns>Client profile or null if not found.</returns>
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

        /// <summary>
        /// Retrieves the profile of an employee user by their ID.
        /// </summary>
        /// <param name="userId">Employee user ID.</param>
        /// <returns>Employee profile or null if not found.</returns>
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
