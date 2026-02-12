using Airline_Ticket_System.Data.Constants;
using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Models;
using Airline_Ticket_System.Models.Account;
using Airline_Ticket_System.Repositories;
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

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
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
        public async Task<IActionResult> Register(RegisterViewModel model)
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
                    FamilyName = model.FamilyName
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, UserRolesEnum.User.ToString());

                    _logger.LogInformation("User registered successfully.");

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
        public async Task<IActionResult> RegisterOperator(RegisterViewModel model)
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
                    FamilyName = model.FamilyName
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, UserRolesEnum.Operator.ToString());

                    _logger.LogInformation("Operator registered successfully.");

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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
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

        // Logout action
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> EditProfile()
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
        public async Task<IActionResult> EditProfile(ProfileViewModel model)
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
        public async Task<IActionResult> Users()
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

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
