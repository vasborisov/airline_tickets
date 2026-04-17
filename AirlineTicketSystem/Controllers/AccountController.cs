using Airline_Ticket_System.Data.Constants;
using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Models;
using Airline_Ticket_System.Models.Account;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Controllers
{ 
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
        }

        // Register action
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Register POST action
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ActionName("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    // Add an error to the model state if the user already exists
                    ModelState.AddModelError(string.Empty, "A user with provided email already exists.");
                    return View(model);
                }

                var user = new ApplicationUser {
                    UserName = model.Email, 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    FamilyName = model.FamilyName,
                    IsActive = true
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, UserRolesEnum.User.ToString());

                    _logger.LogInformation("User registered successfully.");
                    await _emailService.SendWelcomeAsync(user.Email!, $"{user.FirstName} {user.FamilyName}");

                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterOperator()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [ActionName("RegisterOperator")]
        public async Task<IActionResult> RegisterOperatorAsync(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    // Add an error to the model state if the user already exists
                    ModelState.AddModelError(string.Empty, "A user with provided email already exists.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    FamilyName = model.FamilyName,
                    IsActive = true
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, UserRolesEnum.Operator.ToString());

                    _logger.LogInformation("Operator registered successfully.");
                    await _emailService.SendOperatorCreatedAsync(user.Email!, $"{user.FirstName} {user.FamilyName}");

                    return RedirectToAction("Users");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // Login action
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Login")]
        public async Task<IActionResult> LoginAsync(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "This account is disabled. Contact an administrator.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Flight");
                    }

                    if (result.IsLockedOut)
                    {
                        return View("Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                }

            }
            return View(model);
        }

        [ActionName("Logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        [ActionName("EditProfile")]
        public async Task<IActionResult> EditProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var applicationUser = user as ApplicationUser;
            if (applicationUser != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var model = new ProfileViewModel
                {
                    FirstName = applicationUser.FirstName,
                    FamilyName = applicationUser.FamilyName,
                    Email = user.Email,
                    Roles = roles
                };
                return View(model);
            } else
            {
                ModelState.AddModelError(string.Empty, "The user is not of the expected type.");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        [ActionName("EditProfile")]
        public async Task<IActionResult> EditProfileAsync(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var emailExist = await _userManager.FindByEmailAsync(model.Email);
                if (emailExist != null && emailExist.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "Email is already in use by another account.");
                }

                var applicationUser = user as ApplicationUser;
                applicationUser.FirstName = model.FirstName;
                applicationUser.FamilyName = model.FamilyName;  
                applicationUser.UserName = model.Email;
                applicationUser.Email = model.Email;
                
                var result = await _userManager.UpdateAsync(applicationUser);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("EditProfile"); 
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [ActionName("Users")]
        public async Task<IActionResult> UsersAsync()
        {
            var adminRoleId = await _context.Roles
                                .Where(r => r.Name == UserRolesEnum.Admin.ToString())
                                .Select(r => r.Id)
                                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(adminRoleId))
            {
                return View(new List<ApplicationUser>());
            }

            var nonAdminUsers = await _context.Users
                .Where(u => !_context.UserRoles
                    .Where(ur => ur.RoleId == adminRoleId)
                    .Select(ur => ur.UserId)
                    .Contains(u.Id))
                .ToListAsync();

            return View(nonAdminUsers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var current = await _userManager.GetUserAsync(User);
            if (current?.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot change your own active status.";
                return RedirectToAction("Users");
            }

            var target = await _userManager.FindByIdAsync(id);
            if (target == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            if (await _userManager.IsInRoleAsync(target, UserRolesEnum.Admin.ToString()))
            {
                TempData["ErrorMessage"] = "Admin accounts cannot be disabled from this screen.";
                return RedirectToAction("Users");
            }

            target.IsActive = !target.IsActive;
            await _userManager.UpdateAsync(target);
            _logger.LogInformation("User {UserId} IsActive={Active} toggled by {AdminId}", id, target.IsActive, current?.Id);

            await _emailService.SendAccountActiveChangedAsync(
                target.Email!,
                $"{target.FirstName} {target.FamilyName}",
                target.IsActive);

            TempData["SuccessMessage"] = $"User {target.Email} is now {(target.IsActive ? "active" : "inactive")}.";
            return RedirectToAction("Users");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
