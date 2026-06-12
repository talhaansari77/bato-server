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
using BatoClinic.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// CORS controls which frontend apps can call this backend.
// For local development, we allow common local frontend/mobile URLs.
builder.Services.AddCors(options =>
{
    options.AddPolicy("BatoCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:8081",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:8081"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Adds controller support.
// Controllers are classes that expose API endpoints.
builder.Services.AddControllers();

// Adds Swagger API documentation/testing page.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BATO Clinic API",
        Version = "v1",
        Description = "Backend API for BATO Clinic mobile app"
    });

    // Adds JWT Bearer authorization support to Swagger.
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token here. Example: Bearer eyJhbGciOi..."
    });

    // Tells Swagger to apply the Bearer token to protected endpoints.
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
// Catches unexpected exceptions and returns clean JSON errors.
app.UseMiddleware<ExceptionMiddleware>();

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
// CORS should run before Authentication/Authorization for API requests.
app.UseCors("BatoCorsPolicy");
// Authentication must come before Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();