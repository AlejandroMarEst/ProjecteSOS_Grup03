using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProjecteSOS_Grup03API.Models;
using System.Diagnostics.Metrics;

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

        public static void SeedProducts(ModelBuilder builder)
        {
            const string FileName = "Products.json";
            const string FilePath = @"Files/" + FileName;

            FileInfo fileInfo = new FileInfo(FilePath);

            StreamReader reader = fileInfo.OpenText();
            string fileText = reader.ReadToEnd();
            reader.Close();

            List<Product>? products = JsonConvert.DeserializeObject<List<Product>>(fileText);

            if (products != null)
            {
                builder.Entity<Product>().HasData(products);
            }
        }
    }
}
