using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Project_of_Sharan.Models;
using Project_of_Sharan.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Project_of_Sharan.Controllers
{
    public class AccountController : Controller
    {
        private readonly Project_of_SharanContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(Project_of_SharanContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // Registration Action (GET)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid user data.");
                TempData["ErrorMessage"] = "Please provide valid details."; // Error message for SweetAlert
                return View(user);
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.Username == user.Username);

                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Username already taken.");
                    TempData["ErrorMessage"] = "Username already taken."; // Error message for SweetAlert
                    return View(user);
                }

                user.Role = user.Username == "ConestogaCollege" ? "Admin" : "User";

                // Hash the password before saving
                user.Password = _passwordHasher.HashPassword(user, user.Password);

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful. Please log in."; // Success message for SweetAlert
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // Login Action (GET)
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.User.FirstOrDefault(u => u.Username == model.Username);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    TempData["ErrorMessage"] = "Invalid username or password."; // Error message for SweetAlert
                    return View(model);
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    if (user.Role == model.Role) // Ensure selected role matches database role
                    {
                        HttpContext.Session.SetInt32("UserId", user.UserId); // Add UserId to session
                        HttpContext.Session.SetString("Username", user.Username);
                        HttpContext.Session.SetString("Role", user.Role);

                        TempData["SuccessMessage"] = "Login successful. Welcome!"; // Success message for SweetAlert

                        return user.Role == "Admin"
                            ? RedirectToAction("AdminIndex", "Review")
                            : RedirectToAction("MyReviews", "Review");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Incorrect role selected.");
                        TempData["ErrorMessage"] = "Incorrect role selected."; // Error message for SweetAlert
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    TempData["ErrorMessage"] = "Invalid username or password."; // Error message for SweetAlert
                }
            }
            return View(model);
        }

        public async Task<IActionResult> SeedAdmin()
        {
            var adminExists = await _context.User.FirstOrDefaultAsync(u => u.Username == "ConestogaCollege");

            if (adminExists == null)
            {
                var passwordHasher = new PasswordHasher<User>();

                var adminUser = new User
                {
                    Username = "ConestogaCollege",
                    Role = "Admin",
                    Password = passwordHasher.HashPassword(null, "123") // Hash the password correctly
                };

                _context.User.Add(adminUser);
                await _context.SaveChangesAsync();

                return Content("Admin user created successfully.");
            }

            return Content("Admin user already exists.");
        }

        // Logout Action
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have logged out successfully."; // Success message for SweetAlert
            return RedirectToAction("Login");
        }

    }
}
