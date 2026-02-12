namespace Airline_Ticket_System.Data
{

    using System;
    using System.Threading.Tasks;
    using Airline_Ticket_System.Configurations;
    using Airline_Ticket_System.Data.Constants;
    using Airline_Ticket_System.Data.Entities;
    using Airline_Ticket_System.Repositories;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    public static class DatabaseSeedData
    {
        public static async Task Initialize(WebApplication app)
        {

            using (var serviceScope = app.Services.CreateScope())
            {
                using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    dbContext.Database.Migrate();
                    var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var adminOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<AdminUserSettings>>();

                    // Seed roles
                    await SeedRolesAsync(roleManager);

                    // Seed a default admin user
                    await SeedAdminUserAsync(userManager, adminOptions);
                }
            }
        }

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) 
        {
            foreach (UserRolesEnum role in Enum.GetValues(typeof(UserRolesEnum)))
            {
                var roleName = role.ToString();
                
                try
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                Console.WriteLine($"Error: {error.Description}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Role {roleName} created successfully.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Role {roleName} already exists.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating role {roleName}: {ex.Message}");
                }
            }
        }

       public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IOptions<AdminUserSettings> adminOptions)
        {
            var adminSettings = adminOptions.Value;
            var user = await userManager.FindByEmailAsync(adminSettings.Email);
            if (user == null)
            {
                user = new ApplicationUser { 
                    FirstName = adminSettings.Name,
                    UserName = adminSettings.Email, 
                    Email = adminSettings.Email
                };
                var createUser = await userManager.CreateAsync(user, adminSettings.Password);

                if (createUser.Succeeded)
                {
                    await userManager.UpdateAsync(user);
                    await userManager.AddToRoleAsync(user, UserRolesEnum.Admin.ToString());
                }
            }
        }
    }
}
