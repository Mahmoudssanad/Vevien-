using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services;
using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// √ﬂÀ— „‰ ⁄œœ „⁄Ì‰ Œ·«· › —… “„‰Ì… „Õœœ… API Ã«Â“ Ì„‰⁄ «·„” Œœ„ „‰ ÿ·» ‰›” «·‹ Middleware 
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("otpResendPolicy", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 1; // „”„ÊÕ „—Â ﬂ· œﬁÌﬁ…
        limiterOptions.QueueLimit = 0;
    });
});

// Register session service
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

// Register DbContext service
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("constr")));

// Register Identity service
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // «·—„Ê“ œÌ ﬂ·Â« Êﬂ· «·Õ—Ê› Ê«·„”«›«  ﬂ„«‰ UserName field ⁄‘«‰ Ìﬁ»· ›Ì «· 
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Register some services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFollowService, FollowService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRateLimiter();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
