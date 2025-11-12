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

//JWT CONFIGURATION START
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<ApplicationTrackerOnline.Services.JwtOptions>(jwtSection);

var jwtKey = jwtSection.GetValue<string>("Key") ?? throw new Exception("Jwt:Key missing from appsettings.json");
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "ApplicationTrackerOnline";
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "ApplicationTrackerClients";

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

builder.Services.AddScoped<JwtService>();
//JWT CONFIGURATION END


var app = builder.Build();

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

    await next();
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
