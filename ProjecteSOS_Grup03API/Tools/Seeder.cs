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
    }
}
