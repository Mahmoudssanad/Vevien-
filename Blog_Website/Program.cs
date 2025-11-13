using Blog_Website.Hubs;
using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services;
using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// أكثر من عدد معين خلال فترة زمنية محددة API جاهز يمنع المستخدم من طلب نفس الـ Middleware 
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("otpResendPolicy", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 1; // مسموح مره كل دقيقة
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
builder.Services.AddDbContext<AppDbContext>(
    options => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("constr"));
        options.EnableSensitiveDataLogging(); // 👈 دا هيساعدك تشوف القيم اللي سببت الخطأ
    }
);


// Register Identity service
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // الرموز دي كلها وكل الحروف والمسافات كمان UserName field عشان يقبل في ال 
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSignalR();

// Register some services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// When use signalR with comment partial view
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IAccountService, AccountService>();


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

app.MapHub<CommentHub>("/commentHub");
app.MapHub<NotificationHub>("/notificationHub");

app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
