using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Serilog;
using Serilog.Events;
using InventoryManagement.Models;
using InventoryManagement.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure PostgreSQL Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Using connection string: {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
        npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
    });
});

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;  // Make email confirmation optional
    options.SignIn.RequireConfirmedEmail = false;    // Make email confirmation optional
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

// Add email service for account verification
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, InventoryManagement.Services.EmailSender>();

var app = builder.Build();

// Initialize roles and admin user
using (var initScope = app.Services.CreateScope())
{
    var services = initScope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await InventoryManagement.Data.RoleInitializer.InitializeAsync(userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    Console.WriteLine("Running in Development mode");
}

// Global error handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An unhandled exception occurred");
        if (app.Environment.IsDevelopment())
        {
            throw;  // Re-throw in development to see the full error
        }
        context.Response.Redirect("/Home/Error");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var dbScope = builder.Services.BuildServiceProvider().CreateScope())
{
    try
    {
        var dbContext = dbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Use this safely instead of Migrate
        if (!dbContext.Database.CanConnect())
        {
            Console.WriteLine("Cannot connect to database.");
        }
        else if (dbContext.Database.EnsureCreated())
        {
            Console.WriteLine("Database schema created.");
        }
        else
        {
            Console.WriteLine("Database already exists and is accessible.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB Setup Exception: {ex.Message}");
    }
}


Console.WriteLine("App is starting... Ready for requests.");
app.Run();
