using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Airline_Ticket_System.Controllers;
using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Models.Account;
using System.Threading.Tasks;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Airline_Ticket_System.Services.Interfaces;

namespace Airline_Ticket_System.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ILogger<AccountController>> _loggerMock;
        private readonly Mock<IEmailService> _emailServiceMock;

        private readonly ApplicationDbContext _context;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase") // Unique name per test run
            .Options;

            _context = new ApplicationDbContext(options); // Use InMemory database for the context

            // Mock dependencies
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            _loggerMock = new Mock<ILogger<AccountController>>();
            _emailServiceMock = new Mock<IEmailService>();

            // Initialize controller with mocks
            _controller = new AccountController(
                _context, // Using InMemoryDbContext
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _signInManagerMock.Object,
                _loggerMock.Object,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task Register_ReturnsView_WhenModelIsInvalid()
        {
            _controller.ModelState.AddModelError("Email", "Email is required.");

            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "John",
                FamilyName = "Doe",
                ConfirmPassword = "Password123"
            };

            // Act
            var result = await _controller.RegisterAsync(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }


        [Fact]
        public async Task Register_Post_ValidModel_CreatesUserAndRedirects()
        {
            // Arrange
            var controller = new AccountController(
                null, // context is not used directly in Register
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _signInManagerMock.Object,
                _loggerMock.Object,
                _emailServiceMock.Object
            );

            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                FirstName = "John",
                FamilyName = "Doe"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.RegisterAsync(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);

            // Verify that welcome email was sent
            _emailServiceMock.Verify(x => x.SendWelcomeAsync(
                model.Email, 
                $"{model.FirstName} {model.FamilyName}",
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsView_WhenModelIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required.");
            var model = new LoginViewModel
            {
                Email = "",
                Password = "Password123!",
                RememberMe = true
            };

            // Act
            var result = await _controller.LoginAsync(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task Login_ReturnsRedirect_WhenLoginSucceeds()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "password",
                RememberMe = false
            };

            var user = new ApplicationUser { Id = "1", UserName = "test@example.com", Email = "test@example.com" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);

            var signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            _signInManagerMock.Setup(m => m.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false)).ReturnsAsync(signInResult);

            // Act
            var result = await _controller.LoginAsync(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Flight", redirectResult.ControllerName);
        }

        [Fact]
        public async Task EditProfile_ReturnsView_WhenUserIsFound()
        {
            var user = new ApplicationUser
            {
                Id = "1",
                UserName = "test@example.com", // This is the important field
                Email = "test@example.com",    // Ensure Email is set here
                FirstName = "John",
                FamilyName = "Doe"
            };

            // Mock GetUserAsync to return the user
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            var result = await _controller.EditProfileAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ProfileViewModel>(viewResult.Model);

            // Check if the Email is correctly assigned
            Assert.Equal("test@example.com", model.Email);
        }


        [Fact]
        public async Task EditProfile_ReturnsRedirect_WhenUserIsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.EditProfileAsync();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task EditProfilePost_ReturnsRedirect_WhenModelIsValid()
        {
            var user = new ApplicationUser
            {
                Id = "1",
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "John",
                FamilyName = "Doe"
            };

            var model = new ProfileViewModel
            {
                FirstName = "UpdatedFirst",
                FamilyName = "UpdatedLast",
                Email = "test@example.com"
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null); // No email conflict

            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AccountController(
                _context,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _signInManagerMock.Object,
                _loggerMock.Object,
                _emailServiceMock.Object
            );

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            var result = await controller.EditProfileAsync(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("EditProfile", redirectResult.ActionName);

            _userManagerMock.Verify(um => um.UpdateAsync(It.Is<ApplicationUser>(u =>
                u.FirstName == "UpdatedFirst" && u.FamilyName == "UpdatedLast")), Times.Once);
        }

        [Fact]
        public async Task RegisterOperator_Post_ValidModel_CreatesOperatorAndSendsEmail()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "operator@example.com",
                Password = "Operator123!",
                ConfirmPassword = "Operator123!",
                FirstName = "Jane",
                FamilyName = "Operator"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Operator"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.RegisterOperatorAsync(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Users", redirectResult.ActionName);

            // Verify that operator creation email was sent
            _emailServiceMock.Verify(x => x.SendOperatorCreatedAsync(
                model.Email, 
                $"{model.FirstName} {model.FamilyName}",
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatus_ValidUser_SendsStatusChangeEmail()
        {
            // Arrange
            var targetUser = new ApplicationUser
            {
                Id = "target123",
                Email = "target@example.com",
                FirstName = "Target",
                FamilyName = "User",
                IsActive = true
            };

            var currentUser = new ApplicationUser
            {
                Id = "current123",
                Email = "admin@example.com",
                FirstName = "Admin",
                FamilyName = "User"
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);

            _userManagerMock.Setup(x => x.FindByIdAsync("target123"))
                .ReturnsAsync(targetUser);

            _userManagerMock.Setup(x => x.IsInRoleAsync(targetUser, "Admin"))
                .ReturnsAsync(false);

            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Setup TempData
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            // Act
            var result = await _controller.ToggleUserStatus("target123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Users", redirectResult.ActionName);

            // Verify that status change email was sent (user was active, so now inactive)
            _emailServiceMock.Verify(x => x.SendAccountActiveChangedAsync(
                targetUser.Email,
                $"{targetUser.FirstName} {targetUser.FamilyName}",
                false, // Should be toggled from true to false
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Users_ReturnsViewWithNonAdminUsers()
        {
            _context.Users.RemoveRange(_context.Users);
            var adminRole = new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" };
            var userRole = new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" };

            _context.Roles.AddRange(adminRole, userRole);

            var user1 = new ApplicationUser
            {
                Id = "1",
                UserName = "user1@example.com",
                Email = "user1@example.com",
                FirstName = "User",
                FamilyName = "One"
            };
            var user2 = new ApplicationUser
            {
                Id = "2",
                UserName = "user2@example.com",
                Email = "user2@example.com",
                FirstName = "User",
                FamilyName = "Two"
            };
            var adminUser = new ApplicationUser
            {
                Id = "3",
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                FamilyName = "User"
            };

            _context.Users.AddRange(user1, user2, adminUser);

            _context.UserRoles.AddRange(
                new IdentityUserRole<string> { UserId = "1", RoleId = userRole.Id },
                new IdentityUserRole<string> { UserId = "2", RoleId = userRole.Id },
                new IdentityUserRole<string> { UserId = "3", RoleId = adminRole.Id }
            );

            await _context.SaveChangesAsync();

            var result = await _controller.UsersAsync();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ApplicationUser>>(viewResult.Model);
            Assert.Equal(2, model.Count); 
            Assert.DoesNotContain(model, u => u.Email == "admin@example.com");

        }

    }
}

