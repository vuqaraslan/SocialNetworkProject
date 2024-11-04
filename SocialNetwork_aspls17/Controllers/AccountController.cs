using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork_aspls17.Data;
using SocialNetwork_aspls17.Entities;
using SocialNetwork_aspls17.Models;
using SocialNetwork_aspls17.Services;
using System.Globalization;

namespace SocialNetwork_aspls17.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<CustomIdentityUser>? _userManager;
        private readonly RoleManager<CustomIdentityRole>? _roleManager;
        private readonly SignInManager<CustomIdentityUser>? _signInManager;
        private readonly SocialNetworkDbContext? _context;
        private readonly IImageService? _imageService;

        public AccountController(UserManager<CustomIdentityUser>? userManager,
                                 RoleManager<CustomIdentityRole>? roleManager,
                                 SignInManager<CustomIdentityUser>? signInManager,
                                 SocialNetworkDbContext? context,
                                 IImageService? imageService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _imageService = imageService;
        }

        // GET: AccountController
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.File != null)
                {
                    model.ImageUrl = await _imageService.SaveFile(model.File);
                }
                //else
                //{
                //    model.ImageUrl = "lake.png";
                //}
                CustomIdentityUser user = new CustomIdentityUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Image = model.ImageUrl
                };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        CustomIdentityRole role = new CustomIdentityRole
                        {
                            Name = "Admin"
                        };

                        IdentityResult roleResult = await _roleManager.CreateAsync(role);
                        if (!roleResult.Succeeded)
                        {
                            return View(model);
                        }
                    }

                    await _userManager.AddToRoleAsync(user, "Admin");
                    return RedirectToAction("Login", "Account");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = _context.Users.SingleOrDefault(u => u.UserName == model.Username);
                    if (user != null)
                    {
                        user.ConnectTime = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                        user.IsOnline = true;
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid Login !");
                return RedirectToAction("Register", "Account");//Bu menim elave etdiyim koddur ki,eger user Login olanda
                                                               //Succeeded-den basqa cavab qaytarirsa onda Register-e 
                                                               //yonlendirirem
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                user.DisConnectTime = DateTime.Now;
                user.IsOnline = false;
                await _context.SaveChangesAsync();
                await _signInManager.SignOutAsync();
            }
            return RedirectToAction("Login", "Account");
        }



    }
}
