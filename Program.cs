using System.Security.Claims;
using System.Text;
using BatoClinic.Api.Configuration;
using BatoClinic.Api.Data;
using BatoClinic.Api.Entities;
using BatoClinic.Api.Interfaces;
using BatoClinic.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Adds controller support.
// Controllers are classes that expose API endpoints.
builder.Services.AddControllers();

// Adds Swagger API documentation/testing page.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Reads the MySQL connection string from appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string is missing.");
}

// Registers Entity Framework Core with MySQL.
// ServerVersion.AutoDetect checks your MySQL version automatically.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Registers ASP.NET Core Identity.
// Identity handles users, password hashing, roles, and login security.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Reads Jwt section from appsettings.json into JwtSettings class.
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

// Registers our custom token service.
// Scoped means one instance is created per API request.
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure JWT authentication.
// Important: this comes AFTER AddIdentity so JWT becomes the default auth scheme for API endpoints.
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException("JWT settings are missing.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

var app = builder.Build();

// Runs database migrations and inserts default BATO data.
// This creates roles, admin user, branch, categories, and sample services.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await DbSeeder.SeedAsync(context, userManager, roleManager);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disabled for local development to avoid HTTPS port warning.
// app.UseHttpsRedirection();

// Authentication must come before Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();