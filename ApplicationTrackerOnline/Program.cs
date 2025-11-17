using System.Text;
using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;
using ApplicationTrackerOnline.Services;
using Jose;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

//JWT CONFIGURATION START
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<ApplicationTrackerOnline.Services.JwtOptions>(jwtSection);

var jwtKey = jwtSection.GetValue<string>("Key") ?? throw new Exception("Jwt:Key missing from appsettings.json");
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "ApplicationTrackerOnline";
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "ApplicationTrackerClients";


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddScoped<ApplicationStatsService>();

builder.Services.AddAuthentication(options =>
{
    // Leave default cookie login intact for web users
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer("JwtScheme", options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            // Return a completed task to prevent the redirect.
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});


builder.Services.AddScoped<JwtService>();

var app = builder.Build();

app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var requestHost = context.Request.Host.Host?.ToLowerInvariant();
    if (string.IsNullOrEmpty(requestHost) ||
        !(requestHost == "applicationtracker.asiddons.co.uk" ||
          requestHost == "www.applicationtracker.asiddons.co.uk" ||
          requestHost == "localhost"))
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync($"Forbidden\n\nThe domain '{requestHost}' is not allowed.");
        return;
    }
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        // 2. The original call is now inside the 'try' block
        await next();
    }
    catch (Exception ex)
    {
        // 3. If an exception happens deeper in the application, we catch it here
        logger.LogError(ex, "!!!!!! DEEPER EXCEPTION CAUGHT in custom middleware !!!!!!");

        // 4. Re-throw the exception so the application still behaves the same way
        //    and the default error handler can still process it.
        throw;
    }
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages().WithStaticAssets();

app.Run();
