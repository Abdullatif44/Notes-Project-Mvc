using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Notes.Data;
using Notes.Models;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Notes.Models.ViewModels;
using System.Security.Claims;

public class UserController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AppDbContext _context;

    public UserController(AppDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User { Name = model.Name, Email = model.Email, UserName = model.Email, PasswordHash = model.Password };

            // Create user using UserManager
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Redirect to login page
                return RedirectToAction("Login");
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
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        ClaimsPrincipal claimUser = HttpContext.User;
        if (claimUser.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Note");

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                List<Claim> claims = new List<Claim>() {
                  new Claim(ClaimTypes.NameIdentifier, model.Email),
                  new Claim("OtherProperties", "User")
                };
                User us = new();
                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, true, false);

                //var res = await _signInManager.PasswordSignInAsync(user ?? us, model.Password);
                //    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                //        CookieAuthenticationDefaults.AuthenticationScheme);

                //    AuthenticationProperties properties = new AuthenticationProperties
                //    {
                //        AllowRefresh = true
                //    };

                //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                //new ClaimsPrincipal(claimsIdentity), properties);
                if (result.Succeeded)
                    return RedirectToAction("Index", "Note");
                return View(model);
                // Redirect to the home page 
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password");
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string Email)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                if (!string.IsNullOrEmpty(token))
                {
                    // Create URL with the token
                    //var lnkHref = "<a href='" + Url.Action("ResetPassword", "Account", new { email = Email, code = token }, "http") + "'>Reset Password</a>";

                    // send email.
                    //using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                    //{
                    //    smtpClient.EnableSsl = true;
                    //    smtpClient.Credentials = new NetworkCredential("educationamt@gmail.com", "****");

                    //    using (MailMessage mail = new MailMessage("educationamt@gmail.com", Email))
                    //    {
                    //        mail.Subject = "Your changed password";
                    //        mail.Body = "<b>Please find the Password Reset Link. </b><br/>" + lnkHref;

                    //        try
                    //        {
                    //            smtpClient.Send(mail);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            // Log or print the exception details for troubleshooting
                    //            Console.WriteLine(ex.ToString());
                    //        }

                    //    }
                    //}

                }
            }
        }

        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string code, string email)
    {
        ResetPasswordModel model = new ResetPasswordModel();
        model.ReturnToken = code;
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.ReturnToken, model.NewPassword);

                if (result.Succeeded)
                {
                    ViewBag.Message = "Successfully Changed";
                }
                else
                {
                    ViewBag.Message = "Something went horribly wrong!";
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User not found");
            }
        }

        return View(model);
    }

    private string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

        // Use PBKDF2 to hash the password
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(20);

        // Combine the salt and hash
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        // Convert to base64 for storage
        return Convert.ToBase64String(hashBytes);
    }

    private bool VerifyPassword(string enteredPassword, string storedPassword)
    {
        // Convert the stored password from base64
        byte[] hashBytes = Convert.FromBase64String(storedPassword);

        // Extract the salt
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        // Hash the entered password using the stored salt
        var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(20);

        // Compare the computed hash with the stored hash
        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != hash[i])
                return false;
        }

        return true;
    }
}