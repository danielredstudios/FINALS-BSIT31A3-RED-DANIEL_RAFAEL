using BookSwapHub.Application.Interfaces;
using BookSwapHub.Infrastructure.Data;
using BookSwapHub.Infrastructure.Entities;
using BookSwapHub.Infrastructure.Services;
using BookSwapHub.Presentation.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var useInMemory = builder.Configuration.GetValue("UseInMemory", true);
if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("BookSwapHubDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    // Use Sqlite by default from template; swap to SqlServer if desired
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// App services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ISwapService, SwapService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Disable/redirect built-in 2FA pages to keep flows basic
app.Use(async (ctx, next) =>
{
    var p = ctx.Request.Path.Value ?? string.Empty;
    if (p.Contains("/Identity/Account/LoginWith2fa", StringComparison.OrdinalIgnoreCase) ||
        p.Contains("/Identity/Account/Manage/TwoFactorAuthentication", StringComparison.OrdinalIgnoreCase) ||
        p.Contains("/Identity/Account/Manage/EnableAuthenticator", StringComparison.OrdinalIgnoreCase) ||
        p.Contains("/Identity/Account/Manage/ResetAuthenticator", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.Redirect("/");
        return;
    }
    await next();
});

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
