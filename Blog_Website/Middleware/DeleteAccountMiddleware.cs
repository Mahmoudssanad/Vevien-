using Blog_Website.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Blog_Website.Middleware
{
    public class DeleteAccountMiddleware
    {
        private readonly RequestDelegate _next;

        public DeleteAccountMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager)
        {
            if (httpContext.User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(httpContext.User);
                if (user!.IsDeleted && user is not null)
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
