using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using WebApplication1.Models.Authorization;

public class AuthorizationController : Controller
{
    private readonly string accountsFile = Path.Combine(Directory.GetCurrentDirectory(), "accounts.txt");

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Username and Password cannot be empty.";
            return View();
        }

        string hashedPassword = PasswordHasher.HashPassword(password);
        
        string accountData = $"{username}:{hashedPassword}";
        System.IO.File.AppendAllLines("accounts.txt", new[] { accountData });

        ViewBag.Message = "Registration successful! You can now log in.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Username and Password cannot be empty.";
            return View();
        }

        if (System.IO.File.Exists("accounts.txt"))
        {
            var accounts = System.IO.File.ReadAllLines("accounts.txt");
            foreach (var account in accounts)
            {
                var parts = account.Split(':');
                if (parts.Length == 2 && parts[0] == username)
                {
                    string storedHashedPassword = parts[1];
                    if (PasswordHasher.VerifyPassword(password, storedHashedPassword))
                    {
                        // Store username in session
                        HttpContext.Session.SetString("Username", username);

                        // Create claims for the authentication cookie
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, username)
                        };

                        // Create the identity and principal
                        var identity = new ClaimsIdentity(claims, "AuthCookie");
                        var principal = new ClaimsPrincipal(identity);

                        // Sign in using the authentication cookie
                        await HttpContext.SignInAsync("AuthCookie", principal);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.Error = "Invalid username or password.";
                        return View();
                    }
                }
            }
        }

        ViewBag.Error = "Invalid username or password.";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("Username");
        TempData["Message"] = "Logged out successfully.";
        await HttpContext.SignOutAsync("AuthCookie");
        return RedirectToAction("Index", "Home");
    }
}
