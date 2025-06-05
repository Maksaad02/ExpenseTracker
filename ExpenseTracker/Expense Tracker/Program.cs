using Expense_Tracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Dependency Injection (DI) pour ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));

// Configuration Identity avec des comptes par défaut
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configuration des cookies pour Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Page de connexion
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Page d'accès refusé
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// Enregistrer la licence Syncfusion
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2XVhhQlJHfV1dXGVWfFN0QHNcdVt1fldGcDwsT3RfQFhjSH9bdk1gW39adHRVRA==");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Activer l'authentification
app.UseAuthorization();

// Configurer la redirection pour les utilisateurs non authentifiés
app.Use(async (context, next) =>
{
    // Si l'utilisateur n'est pas authentifié
    if (!context.User.Identity.IsAuthenticated)
    {
        // Si l'utilisateur tente d'accéder à une page autre que Login ou Register, redirigez-le vers la page de connexion
        if (!context.Request.Path.StartsWithSegments("/Identity/Account/Login") 
            && !context.Request.Path.StartsWithSegments("/Identity/Account/Register"))
        {
            context.Response.Redirect("/Identity/Account/Login");
            return;
        }

        // Si l'utilisateur est sur la page de login mais qu'il n'a pas de compte, redirigez-le vers la page d'inscription
        if (context.Request.Path.StartsWithSegments("/Identity/Account/Login"))
        {
            // Vous pouvez vérifier ici si l'utilisateur a tenté de se connecter avec un mauvais identifiant
            // Et le rediriger vers la page d'inscription si nécessaire (cela dépend de vos besoins)
            var query = context.Request.Query;
            if (query.ContainsKey("failed") && query["failed"] == "true")
            {
                // Redirection vers la page d'inscription
                context.Response.Redirect("/Identity/Account/Register");
                return;
            }
        }
    }

    // Continue avec les autres middlewares si l'utilisateur est authentifié ou s'il est sur la bonne page
    await next();
});


// Définir le routage par défaut
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages(); // Razor Pages pour Identity

app.Run();