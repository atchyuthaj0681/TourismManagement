using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace TourismManagement.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Ensure roles exist
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                        throw new Exception($"Failed to create role: {role}");
                }
            }

            // 2. Seed default Admin user
            var adminEmail = "atchyuthaj@abc.abc";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NormalizedUserName = adminEmail.ToUpper(),
                    NormalizedEmail = adminEmail.ToUpper(),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Name = "Admin User",
                    Nationality = "CountryName",
                    PassportNumber = "A1234567"
                };

                var createUserResult = await userManager.CreateAsync(user, "Atchyuth@j110");

                if (createUserResult.Succeeded)
                {
                    // Assign ONLY Admin role to this user
                    var addToRoleResult = await userManager.AddToRoleAsync(user, "Admin");
                    if (!addToRoleResult.Succeeded)
                        throw new Exception($"Failed to add admin user to role.");
                }
                else
                {
                    throw new Exception($"Failed to create admin user.");
                }
            }
        }
    }
}
