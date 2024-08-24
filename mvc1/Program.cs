using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using mvc1.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure la base de donn�es
builder.Services.AddDbContext<SchoolContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// Ajoute les services pour les contr�leurs et les vues
builder.Services.AddControllersWithViews();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuration des cookies pour qu'ils soient s�curis�s
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict; // Pr�venir les attaques CSRF en utilisant SameSite
    options.Secure = CookieSecurePolicy.Always; // Assurez-vous que les cookies ne sont envoy�s que via HTTPS
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always; // Emp�cher l'acc�s par JavaScript
});

var app = builder.Build();

// Applique la politique de cookie avant toute chose
app.UseCookiePolicy();

// Initialisation de la base de donn�es
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SchoolContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

// Configure le pipeline de traitement des requ�tes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enforce HTTPS
}

app.UseHttpsRedirection(); // Redirige HTTP vers HTTPS
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
