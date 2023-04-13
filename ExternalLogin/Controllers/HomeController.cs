using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ExternalLogin.Auth;
using ExternalLogin.Context;
using Microsoft.AspNetCore.Mvc;
using ExternalLogin.Models;
using ExternalLogin.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace ExternalLogin.Controllers
{
    public class HomeController : Controller
    {
        private readonly LoginContext _context;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(LoginContext context, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }



        [Route("", Name = "Index")]
        public IActionResult Index()
        {
            var users = _context.Users.ToList();

            return View(users);
        }

        [Route("register", Name = "RegisterGet")]
        public IActionResult Register()
        {
            return View();
        }

        [Route("register", Name = "RegisterPost")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            _context.Users.Add(user);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        [Route("login", Name = "LoginGet")]
        public IActionResult Login()
        {
            return View();
        }

        [Route("login", Name = "LoginPost")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Login(User user)
        {
            var existUser = _context.Users.SingleOrDefault(s => s.UserName == user.UserName && s.Password == user.Password);

            if (existUser != null)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier,existUser.UserId.ToString()),
                    new Claim(ClaimTypes.Name,existUser.UserName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                var properties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                HttpContext.SignInAsync(principal, properties);

                return RedirectToAction("Index");
            }

            TempData["NotExist"] = "کاربری با مشخصات شما یافت نشد";

            return View();
        }


        [Route("sign-out", Name = "SignOut")]
        public IActionResult SignOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        [Route("provider/{provider}")]
        public IActionResult GetProvider(string provider)
        {
            var redirectUrl = Url.RouteUrl("ExternalLogin", Request.Scheme);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }


        [Route("external-login", Name = "ExternalLogin")]
        public IActionResult ExternalLogin()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var user = _context.Users.SingleOrDefault(s => s.Email == userEmail);

            if (user != null)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
                    new Claim(ClaimTypes.Name,user.UserName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                var properties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                HttpContext.SignInAsync(principal, properties);
                
            }

            return RedirectToRoute("Index");
        }
    }
}
