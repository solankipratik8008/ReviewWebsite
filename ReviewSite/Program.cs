using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReviewSite.Data;
using ReviewSite.Models;

var builder = WebApplication.CreateBuilder(args);

var databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "Sqlite");
var connectionString = builder.Configuration.GetConnectionString("ReviewSiteContext")
    ?? throw new InvalidOperationException("Connection string 'ReviewSiteContext' not found.");

builder.Services.AddDbContext<ReviewSiteContext>(options =>
{
    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReviewSiteContext>();

    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        context.Database.Migrate();
    }
    else
    {
        context.Database.EnsureCreated();
    }

    SeedUsers(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void SeedUsers(ReviewSiteContext context)
{
    var passwordHasher = new PasswordHasher<User>();

    EnsureUser(context, passwordHasher, "ConestogaCollege", "123", "Admin");
    EnsureUser(context, passwordHasher, "user1", "password", "User");

    context.SaveChanges();
}

static void EnsureUser(
    ReviewSiteContext context,
    PasswordHasher<User> passwordHasher,
    string username,
    string password,
    string role)
{
    var user = context.User.FirstOrDefault(u => u.Username == username);

    if (user == null)
    {
        user = new User
        {
            Username = username,
            Role = role
        };
        user.Password = passwordHasher.HashPassword(user, password);
        context.User.Add(user);
        return;
    }

    user.Role = role;

    var passwordIsValid = false;

    try
    {
        passwordIsValid = passwordHasher.VerifyHashedPassword(user, user.Password, password)
            != PasswordVerificationResult.Failed;
    }
    catch (FormatException)
    {
        passwordIsValid = false;
    }

    if (!passwordIsValid)
    {
        user.Password = passwordHasher.HashPassword(user, password);
    }
}
