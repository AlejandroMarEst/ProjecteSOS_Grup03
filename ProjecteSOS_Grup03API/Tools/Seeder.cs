using Microsoft.AspNetCore.Identity;
using ProjecteSOS_Grup03API.Models;

namespace ProjecteSOS_Grup03API.Tools
{
    public static class Seeder
    {
        public static async Task CreateInitialRoles(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "Employee", "Client" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        public static async Task SeedAdmins(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();

            var admin = new Employee
            {
                Name = "Admin1",
                UserName = "admin@dynamx.com",
                Email = "admin@dynamx.com",
                Salary = 3000,
                ManagerId = null,
                IsAdmin = true,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                PhoneNumber = "167890"
            };
            string password = "Admin1234!";

            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                var result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
