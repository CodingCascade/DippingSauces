using DohFlo.Data;
using DohFlo.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core with the SQL Server connection string.
builder.Services.AddDbContext<DohFloContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DohFloDb")));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Configure Identity with strong password and lockout
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.SignIn.RequireConfirmedAccount = false;
})
 .AddEntityFrameworkStores<DohFloContext>()
 .AddDefaultTokenProviders();

// Configure the application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.Cookie.Name = "DohFlo.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }

        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }

        return Task.CompletedTask;
    };
});

// Configure antiforgery protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Register the three authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(FinanceAccess.ReadPolicy, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(FinanceAccess.AdminRole, FinanceAccess.StandardUserRole));

    options.AddPolicy(FinanceAccess.WritePolicy, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(FinanceAccess.AdminRole, FinanceAccess.StandardUserRole));

    options.AddPolicy(FinanceAccess.AdminPolicy, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(FinanceAccess.AdminRole));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapControllers();
app.MapControllerRoute(name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// The database migration is applied manually before enabling bootstrap.
if (builder.Configuration.GetValue<bool>("Bootstrap:Enabled"))
{
    using var scope = app.Services.CreateScope();
    await IdentityBootstrap.InitializeAsync(scope.ServiceProvider,
        builder.Configuration);
}

app.Run();

public partial class Program { } // For HTTP tests