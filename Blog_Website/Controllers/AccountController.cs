using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace Blog_Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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

                if(email != null)
                {
                    ModelState.AddModelError("", "Email aleardy exist");
                    return View(model);
                }

                // Save user data in session
                HttpContext.Session.SetString("RegisterData", JsonConvert.SerializeObject(model));

                #region Generate, send and save OTP

                // Generate otp
                var otp = new Random().Next(100000, 999999).ToString();

                // send otp user
                await _emailService.SendEmailAsync(model.Email, "Confirm your email on Blog Website",
                    $"Welcome {model.UserName}! \n Your OTP is: <b>{otp}</b>");

                // Save opt in database
                var newOTP = new OTP
                {
                    Code = otp,
                    Email = model.Email,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _emailService.AddAsync(newOTP);
                #endregion

                return RedirectToAction("VerifyOtp", new { email = model.Email, flow = "Register" });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyOtp(string email, string flow)
        {
            return View(new OtpViewModel { Email = email, Flow = flow});
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpViewModel otpModel)
        {
            var found = await _emailService.FindByEmailAsync(otpModel.Email);

            if(found != null)
            {
                if(otpModel.Otp == found.Code && found.ExpiryTime > DateTime.UtcNow)
                {
                    if(otpModel.Flow == "Register")
                    {
                        // get user data from session
                        var registerData = JsonConvert.DeserializeObject<AccountViewModel>
                            (HttpContext.Session.GetString("RegisterData"));

                        var user = new ApplicationUser
                        {
                            UserName = registerData.UserName,
                            Email = registerData.Email,
                            Address = registerData.Address,
                            Gender = registerData.Gender,
                            Birthdate = registerData.Birthdate,
                            PhoneNumber = registerData.PhoneNumber
                        };

                        // Create user in database
                        var result = await _userManager.CreateAsync(user, registerData.Password);

                        if (result.Succeeded)
                        {
                            found.IsUsed = true;
                            await _emailService.UpdateAsync(found);

                            await _signInManager.SignInAsync(user, registerData.RememberMe);
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    if(otpModel.Flow == "ForgetPassword")
                    {
                        return RedirectToAction("ResetPassword", new {email = otpModel.Email});
                    }
                }
            }
            ModelState.AddModelError("", "Not correct or expired OTP");
            return View(otpModel);
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
                        await _emailService.SendEmailAsync(user.Email,
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
        public async Task<IActionResult> ForgetPassword()
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
        public async Task<IActionResult> ResetPassword(string email)
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
