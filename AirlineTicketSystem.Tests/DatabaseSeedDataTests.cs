using Airline_Ticket_System.Configurations;
using Airline_Ticket_System.Data;
using Airline_Ticket_System.Data.Constants;
using Airline_Ticket_System.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Airline_Ticket_System.Tests
{
    public class DatabaseSeedDataTests
    {
        [Fact]
        public async Task SeedRolesAsync_CreatesMissingRoles()
        {
            var mockRoleManager = GetMockRoleManager();

            mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            mockRoleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            await DatabaseSeedData.SeedRolesAsync(mockRoleManager.Object);

            foreach (var role in Enum.GetNames(typeof(UserRolesEnum)))
            {
                mockRoleManager.Verify(r => r.RoleExistsAsync(role), Times.Once);
                mockRoleManager.Verify(r => r.CreateAsync(It.Is<IdentityRole>(ir => ir.Name == role)), Times.Once);
            }
        }

        [Fact]
        public async Task SeedRolesAsync_RolesAlreadyExist_DoNothing()
        {
            var mockRoleManager = GetMockRoleManager();

            mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true); // All roles exist

            await DatabaseSeedData.SeedRolesAsync(mockRoleManager.Object);

            mockRoleManager.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
        }

        [Fact]
        public async Task SeedRolesAsync_RoleCreationFails_LogsErrors()
        {
            var mockRoleManager = GetMockRoleManager();

            mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            mockRoleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

            await DatabaseSeedData.SeedRolesAsync(mockRoleManager.Object);

            mockRoleManager.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Exactly(Enum.GetNames(typeof(UserRolesEnum)).Length));
        }

        [Fact]
        public async Task SeedAdminUserAsync_UserDoesNotExist_CreatesUserAndAssignsRole()
        {
            var mockUserManager = GetMockUserManager();
            var adminSettings = Options.Create(new AdminUserSettings
            {
                Email = "admin@example.com",
                Password = "Password123!",
                Name = "AdminUser"
            });

            mockUserManager.Setup(m => m.FindByEmailAsync(adminSettings.Value.Email))
                .ReturnsAsync((ApplicationUser)null);

            mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), adminSettings.Value.Password))
                .ReturnsAsync(IdentityResult.Success);

            await DatabaseSeedData.SeedAdminUserAsync(mockUserManager.Object, adminSettings);

            mockUserManager.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(u =>
                u.Email == adminSettings.Value.Email &&
                u.UserName == adminSettings.Value.Email &&
                u.FirstName == adminSettings.Value.Name
            ), adminSettings.Value.Password), Times.Once);

            mockUserManager.Verify(m => m.AddToRoleAsync(It.Is<ApplicationUser>(u => u.Email == adminSettings.Value.Email), UserRolesEnum.Admin.ToString()), Times.Once);
        }

        [Fact]
        public async Task SeedAdminUserAsync_UserAlreadyExists_SkipCreation()
        {
            var mockUserManager = GetMockUserManager();
            var existingUser = new ApplicationUser { Email = "admin@example.com" };

            var adminSettings = Options.Create(new AdminUserSettings
            {
                Email = "admin@example.com",
                Password = "Password123!",
                Name = "AdminUser"
            });

            mockUserManager.Setup(m => m.FindByEmailAsync(adminSettings.Value.Email))
                .ReturnsAsync(existingUser);

            await DatabaseSeedData.SeedAdminUserAsync(mockUserManager.Object, adminSettings);

            mockUserManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SeedAdminUserAsync_CreationFails_LogsError()
        {
            var mockUserManager = GetMockUserManager();
            var adminSettings = Options.Create(new AdminUserSettings
            {
                Email = "admin@example.com",
                Password = "Password123!",
                Name = "AdminUser"
            });

            mockUserManager.Setup(m => m.FindByEmailAsync(adminSettings.Value.Email))
                .ReturnsAsync((ApplicationUser)null);

            mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), adminSettings.Value.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error creating user" }));

            await DatabaseSeedData.SeedAdminUserAsync(mockUserManager.Object, adminSettings);

            mockUserManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), adminSettings.Value.Password), Times.Once);
            mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }


        private static Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
        }

        private static Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
