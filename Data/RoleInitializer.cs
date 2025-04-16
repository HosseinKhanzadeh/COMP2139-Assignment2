using Microsoft.AspNetCore.Identity;
using InventoryManagement.Models;
using System;
using System.Threading.Tasks;

namespace InventoryManagement.Data
{
    public static class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string adminEmail = "admin@inventory.com";
            string adminPassword = "Admin@123";

            // Create Admin role if it doesn't exist
            if (await roleManager.FindByNameAsync("Admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create Regular User role if it doesn't exist
            if (await roleManager.FindByNameAsync("Regular User") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Regular User"));
            }

            // Create admin user if it doesn't exist
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@inventory.com",
                    Email = "admin@inventory.com",
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    PhoneNumber = "1234567890",  // Required!
                    PhoneNumberConfirmed = true,
                    Address = "123 Admin Lane",   // Required!
                    PreferredCategories = "[]",
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
} 