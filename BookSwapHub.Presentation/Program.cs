using BookSwapHub.Application.Interfaces;
using BookSwapHub.Infrastructure.Data;
using BookSwapHub.Infrastructure.Entities;
using BookSwapHub.Infrastructure.Services;
using BookSwapHub.Presentation.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var useInMemory = builder.Configuration.GetValue("UseInMemory", false);
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
    options.Password.RequiredLength = 8;
})
.AddPasswordValidator<ComplexPasswordValidator>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// App services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ISwapService, SwapService>();

builder.Services.AddControllersWithViews();

// Configure request size limits for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB limit
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBoundaryLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configure Kestrel server limits
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB limit
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
});

var app = builder.Build();

// Apply EF Core migrations automatically in non-InMemory mode (code-first)
if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

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
