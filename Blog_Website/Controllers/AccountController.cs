using Blog_Website.Enums;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;

namespace Blog_Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailService emailService, IAccountService accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _accountService = accountService;
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
            if (!ModelState.IsValid)
                return View(model);

            var emailExseting = await _userManager.FindByEmailAsync(model.Email!);

            if(emailExseting is not null)
            {
                ModelState.AddModelError("", "Email already exist");
                return View(model);
            }

            // Save user data in session
            HttpContext.Session.SetString("RegisterData", JsonConvert.SerializeObject(model));

            #region Generate, send and save OTP
            await _emailService.GenerateAndSendOtpAsync(model.Email!, model.UserName!);
            #endregion

            return RedirectToAction("VerifyOtp", new { email = model.Email, flow = OtpFlow.Register });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email, OtpFlow flow)
        {
            var registerData = HttpContext.Session.GetString("RegisterData");

            if (flow == OtpFlow.Register && registerData is null)
            {
                // المستخدم حاول يدخل مباشرة باللينك
                TempData["Error"] = "You cannot access this page directly.";
                return RedirectToAction("Register");
            }
            return View(new OtpViewModel { Email = email, Flow = flow});
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpViewModel otpModel)
        {
            if (!ModelState.IsValid)
                return View(otpModel);

            var isValid = await _emailService.ValidateOtpAsync(otpModel.Email!, otpModel.Otp!, otpModel.Flow);
            if (!isValid)
            {
                ModelState.AddModelError("", "OTP is incorrect or expired.");
                return View(otpModel);
            }

            switch (otpModel.Flow)
            {
                case OtpFlow.Register:
                    var registerDataJson = HttpContext.Session.GetString("RegisterData")!;
                    if (registerDataJson == null)
                        return RedirectToAction("Register", "Account");

                    var registerData = JsonConvert.DeserializeObject<AccountViewModel>(registerDataJson);
                    var result = await _accountService.CreateAsync(registerData!);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Error creating account");
                        return View(otpModel);
                    }

                    await _signInManager.SignInAsync(result.User!, registerData!.RememberMe);
                    return RedirectToAction("Index", "Home");

                case OtpFlow.ForgetPassword:
                    return RedirectToAction("ResetPassword", new { email = otpModel.Email });

                default:
                    ModelState.AddModelError("", "Invalid OTP flow.");
                    return View(otpModel);
            }
        }

        [HttpGet]
        [EnableRateLimiting("otpResendPolicy")]
        public async Task<IActionResult> ResendOtp(string email, OtpFlow flow)
        {
            var result = await _emailService.ResendOtpAsync(email, flow);

            TempData[result.Success ? "Message" : "Error"] = result.Message;

            return RedirectToAction("VerifyOtp", new { email, flow });
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

                if(found != null && !found.IsDeleted)
                {
                    var passwordCheck = await _userManager.CheckPasswordAsync(found, model.Password);

                    if (passwordCheck)
                    {
                        await _signInManager.SignInAsync(found, model.RememberMe);
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("", "Invalid email or password");
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

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var found = await _userManager.FindByEmailAsync(model.Email!);

                if(found == null)
                {
                    ModelState.AddModelError("", "Invaild email");
                    return View(model);
                }

                await _emailService.GenerateAndSendOtpAsync(model.Email!, found.UserName!);

                return RedirectToAction("VerifyOtp", new { email = model.Email, flow = OtpFlow.ForgetPassword });
            }
            ModelState.AddModelError("", "Invaild Email");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (email == null)
                return RedirectToAction("ForgetPassword");

            return View(new ResetPasswordViewModel{ Email = email });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var found = await _userManager.FindByEmailAsync(model.Email);

                if(found == null)
                {
                    ModelState.AddModelError("", "Invalid Email");
                    return View(model);
                }

                // Create Token
                var token = await _userManager.GeneratePasswordResetTokenAsync(found);

                // Change Password
                var result = await _userManager.ResetPasswordAsync(found, token, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
    }
}