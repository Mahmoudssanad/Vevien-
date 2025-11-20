using Blog_Website.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Blog_Website.Middleware
{
    public class DeleteAccountMiddleware
    {
        private readonly RequestDelegate _next;

        // Middleware => Singlton وال Scoped لانهم UserManager, SinInManager لل dependancy injection معملناش 
        // نفسه HttpContext من ال UserManager علشان كدا اخدنا ال 
        public DeleteAccountMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var _userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var _signInManager = httpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();


            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(httpContext.User);

                if (user != null && user.IsDeleted)
                {
                    await _signInManager.SignOutAsync();
                    httpContext.Response.Redirect("/Account/Login");
                    return;
                }
            }

            await _next(httpContext); // move to next middleware
        }

    }
}
