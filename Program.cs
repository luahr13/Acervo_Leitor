using Acervo_Leitor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// DETECTAR AMBIENTE
// ======================================================

// Visual Studio / dotnet run environment detection
var isDevelopment = builder.Environment.IsDevelopment();

// Detectar EXE publicado (executável standalone, não dotnet run)
// Verificações mais robustas:
// 1. EntryAssembly não é null (programa executado diretamente)
// 2. Não contém "dotnet.exe" ou "dotnet" na linha de comando
// 3. O caminho base não contém "bin\Debug" ou "bin\Release"
var isPublishedExe = !Environment.CommandLine.Contains("dotnet") 
                     && !AppContext.BaseDirectory.Contains("bin\\Debug")
                     && !AppContext.BaseDirectory.Contains("bin/Debug");

// Caminho do banco demo
var demoDbPath = Path.Combine(AppContext.BaseDirectory, "demo.db");

// Regra FINAL do modo demo (apenas se for EXE publicado E existir demo.db)
var isDemo = isPublishedExe && File.Exists(demoDbPath);

// LOGS NO CONSOLE - informações detalhadas
Console.WriteLine("╔════════════════════════════════════════╗");
Console.WriteLine(isDevelopment ? "║ 🧠 DESENVOLVIMENTO (Visual Studio)   ║" : "║ 📦 PUBLICADO (Arquivo .exe)          ║");
Console.WriteLine(isDemo ? "║ 🧪 MODO DEMO (SQLite)               ║" : "║ 💾 MODO PRODUÇÃO (SQL Server)       ║");
Console.WriteLine($"║ Ambiente: {builder.Environment.EnvironmentName,-27} ║");
Console.WriteLine($"║ BaseDirectory: {AppContext.BaseDirectory.Substring(0, Math.Min(25, AppContext.BaseDirectory.Length)),-21}... ║");
Console.WriteLine("╚════════════════════════════════════════╝");

// ======================================================
// CONFIGURAÇÃO DO BANCO
// ======================================================

if (isDemo)
{
    try
    {
        var sqliteConn = $"Data Source={demoDbPath}";
        Console.WriteLine($"✅ Conectando ao SQLite: {demoDbPath}");
        builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(sqliteConn));
        builder.WebHost.UseUrls("http://localhost:5000");
        Console.WriteLine("✅ URL configurada: http://localhost:5000");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ ERRO SQLite: " + ex.Message);
        throw;
    }
}
else
{
    var sql = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("❌ ERRO: DefaultConnection não foi encontrada em appsettings.json!");

    Console.WriteLine($"✅ Conectando ao SQL Server");
    Console.WriteLine($"   ConnectionString: {sql.Substring(0, Math.Min(60, sql.Length))}...");
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(sql));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ======================================================
// IDENTITY
// ======================================================

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

// ======================================================
// AUTH GLOBAL
// ======================================================

builder.Services.AddControllersWithViews(o =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    o.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddRazorPages(o =>
{
    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
});

var app = builder.Build();

// ======================================================
// SEED DE USUÁRIOS
// ======================================================

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

try
{
    db.Database.EnsureCreated();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Criar role Admin se não existir
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    // Seed para MODO DEMO (.exe com demo.db)
    if (isDemo)
    {
        Console.WriteLine("🌱 Seedando usuário DEMO...");
        var demoEmail = "demo@demo.com";
        if (await userManager.FindByEmailAsync(demoEmail) == null)
        {
            var demoUser = new IdentityUser { UserName = demoEmail, Email = demoEmail };
            await userManager.CreateAsync(demoUser, "Demo123");
            await userManager.AddToRoleAsync(demoUser, "Admin");
            Console.WriteLine($"✅ Usuário DEMO criado: {demoEmail} / Senha: Demo123");
        }
    }
    // Seed para MODO DESENVOLVIMENTO (Visual Studio / dotnet run)
    else if (isDevelopment)
    {
        Console.WriteLine("🌱 Seedando usuário DESENVOLVIMENTO...");
        var devEmail = "dev@dev.com";
        if (await userManager.FindByEmailAsync(devEmail) == null)
        {
            var devUser = new IdentityUser { UserName = devEmail, Email = devEmail };
            await userManager.CreateAsync(devUser, "Dev123");
            await userManager.AddToRoleAsync(devUser, "Admin");
            Console.WriteLine($"✅ Usuário DEV criado: {devEmail} / Senha: Dev123");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("❌ ERRO SEED: " + ex.Message);
}

// ======================================================
// PIPELINE
// ======================================================

if (isDevelopment)
{
    Console.WriteLine("✅ Usando DeveloperExceptionPage (Desenvolvimento)");
    app.UseMigrationsEndPoint();
}
else
{
    Console.WriteLine("✅ Usando ExceptionHandler customizado (Produção)");
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!isDemo) // no demo não força HTTPS
{
    app.UseHttpsRedirection();
    Console.WriteLine("✅ HTTPS Redirection ativado");
}
else
{
    Console.WriteLine("⚠️  HTTPS Redirection desativado (Modo Demo)");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// ======================================================
// ENDPOINT DE DIAGNOSTICS (Debug)
// ======================================================

app.MapGet("/diagnostics", () => new
{
    environment = new
    {
        isDevelopment,
        isPublishedExe,
        isDemo,
        dotnetEnvironment = builder.Environment.EnvironmentName,
        baseDirectory = AppContext.BaseDirectory,
        demoDbExists = File.Exists(demoDbPath),
        commandLine = Environment.CommandLine.Substring(0, Math.Min(100, Environment.CommandLine.Length))
    },
    database = new
    {
        mode = isDemo ? "SQLite (Demo)" : "SQL Server (Produção)",
        demoDbPath = demoDbPath,
        demoDbExists = File.Exists(demoDbPath)
    },
    timestamp = DateTime.Now
}).WithName("Diagnostics");

// ======================================================
// ABRIR NAVEGADOR AUTOMATICAMENTE NO EXE
// ======================================================

if (isDemo)
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://localhost:5000",
                UseShellExecute = true
            });
        }
        catch { }
    });
}

app.Run();