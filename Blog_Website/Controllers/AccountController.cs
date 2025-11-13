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
        public async Task<IActionResult> ResendOtp(string email, string flow)
        {
            var found = await _emailService.FindByEmailAsync(email);


            if(found != null && !found.IsUsed ) // متمش استخدمها قبل كدا otp بتاكد ان ال 
            {
                var otp = new Random().Next(100000, 999999).ToString();
                found.ExpiryTime = DateTime.UtcNow.AddMinutes(2);
                found.Code = otp;
                found.IsUsed = false;
                await _emailService.UpdateAsync(found);

                if(flow == "Register")
                {
                    try
                    {
                        // مش فاضيه session عشان اتاكد ان ال 
                        var registerDataJson = HttpContext.Session.GetString("RegisterData");

                        if (!string.IsNullOrEmpty(registerDataJson))
                        {
                            var registerData = JsonConvert.DeserializeObject<AccountViewModel>(registerDataJson);

                            await _emailService.SendEmailAsync(email, "Resend verfication code",
                                $"Welcome {registerData.UserName} \n your code is: {otp}");
                        }
                        else
                        {
                            await _emailService.SendEmailAsync(email, "Resend verification code",
                        $"Your verification code is: {otp}");
                        }

                        TempData["Message"] = "A new OTP has been sent to your email. enjouy";

                        return RedirectToAction("VerifyOtp", new { Email = email, Flow = flow });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                        //TempData["Error"] = "Failed to send email. Please try again.";
                    }
                }

                else if (flow == "ForgetPassword")
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        await _emailService.SendEmailAsync(user.Email!,
                            "Forget password - Vivena website",
                            $"Welcome {user.UserName}! \n Your OTP is: {otp}");

                        TempData["Message"] = "A new OTP has been sent to your email.";

                        return RedirectToAction("VerifyOtp", new { email = user.Email, flow = "ForgetPassword" });
                    }
                    TempData["Message"] = "Email not found.";
                    return RedirectToAction("ForgetPassword");
                }

            }
            TempData["Message"] = "Incorrect, Please try again";
            return RedirectToAction("VerifyOtp", new { Email = email, Flow = flow });
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
                var found = await _userManager.FindByEmailAsync(model.Email);

                if(found == null)
                {
                    ModelState.AddModelError("", "Invaild email");
                    return View(model);
                }

                var otp = new Random().Next(100000, 999999).ToString();

                var newOtp = new OTP
                {
                    Code = otp,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(3),
                    IsUsed = false,
                    Email = model.Email
                };
                await _emailService.AddAsync(newOtp);

                await _emailService.SendEmailAsync(model.Email, "Viven Blog", $"Welcome {found.UserName} your code is: {otp}");

                TempData["Message"] = "A new OTP has been sent to your email.";

                return RedirectToAction("VerifyOtp", new { email = model.Email, flow = "ForgetPassword" });
            }
            ModelState.AddModelError("", "Invaild Email");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel{ Email = email });
        }

        [HttpPost]
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

                var token = await _userManager.GeneratePasswordResetTokenAsync(found);

                var result = await _userManager.ResetPasswordAsync(found, token, model.Password);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password change successfully";
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
