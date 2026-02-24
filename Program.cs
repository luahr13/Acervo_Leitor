using Acervo_Leitor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Detecta modo demo via env var
var isDemo = Environment.GetEnvironmentVariable("DEMO_MODE") == "true";
if (isDemo)
    builder.Environment.EnvironmentName = "Demo";

// Config padrão (não sobrescrever pipeline do .NET)
if (builder.Environment.EnvironmentName == "Demo")
{
    builder.Configuration.AddJsonFile("appsettings.Demo.json", optional: false);
}

// DB config
if (builder.Environment.EnvironmentName == "Demo")
{
    Console.WriteLine("🧪 MODO DEMO - SQLITE");

    var sqlitePath = Path.Combine(AppContext.BaseDirectory, "demo.db");
    var sqliteConn = $"Data Source={sqlitePath}";

    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(sqliteConn));
    builder.WebHost.UseUrls("http://localhost:5000");
}
else
{
    Console.WriteLine("🧠 DEV - SQL SERVER");

    var sql = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("DefaultConnection missing");

    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(sql));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
    o.Password.RequireDigit = false;
    o.Password.RequireUppercase = false;
    o.Password.RequireLowercase = false;
    o.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Auth global
builder.Services.AddControllersWithViews(o =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    o.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddRazorPages(o =>
{
    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
});

var app = builder.Build();

// Seed Demo
async Task SeedDemoAsync()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var email = "anailda@demo.com";
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new IdentityUser { UserName = email, Email = email };
        await userManager.CreateAsync(user, "Anailda@");
        await userManager.AddToRoleAsync(user, "Admin");
    }
}

if (builder.Environment.EnvironmentName == "Demo")
    await SeedDemoAsync();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else if (!builder.Environment.IsEnvironment("Demo"))
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!builder.Environment.IsEnvironment("Demo"))
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Abrir navegador após subir
app.Lifetime.ApplicationStarted.Register(() =>
{
    if (builder.Environment.EnvironmentName == "Demo")
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "http://localhost:5000",
            UseShellExecute = true
        });
    }
});

app.Run();