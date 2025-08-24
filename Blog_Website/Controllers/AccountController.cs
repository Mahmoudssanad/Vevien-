using Blog_Website.Models.Entities;
using Blog_Website.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace Blog_Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var email = await _userManager.FindByEmailAsync(model.Email);
                if (email == null)
                {
                    ApplicationUser user = new ApplicationUser();

                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;
                    user.Birthdate = model.Birthdate;
                    user.PasswordHash = model.Password;
                    user.Gender = model.Gender;

                    // Save the new register in database
                    var newUser = await _userManager.CreateAsync(user, model.Password); // عشان تتهيش Password حطيت هنا ال 
                    if (newUser.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, model.RememberMe);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "All recordes must filed");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email already exist");
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
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var found = await _userManager.FindByEmailAsync(model.Email);

                if(found != null)
                {
                    var passwordCheck = await _userManager.CheckPasswordAsync(found, model.Password);

                    if (passwordCheck)
                    {
                        await _signInManager.SignInAsync(found, model.RememberMe);
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("", "Email or password error");
                return View("Login", model);
            }
            return View("Login", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Json ( new {success = true});
        }
    }
}
