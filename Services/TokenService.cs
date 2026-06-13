using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BatoClinic.Api.Configuration;
using BatoClinic.Api.Entities;
using BatoClinic.Api.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;


namespace BatoClinic.Api.Services;

// TokenService creates JWT tokens for logged-in users.
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public string CreateRefreshToken()
    {
        // RandomNumberGenerator creates a secure random token.
        // This token is sent to the mobile app once, and only its hash is saved in DB.
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        // We do not store refresh tokens directly in DB.
        // We store a SHA256 hash, similar idea to not storing plain passwords.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(bytes);
    }

    // IOptions<JwtSettings> gives us Jwt values from appsettings.json.
    public TokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string CreateToken(ApplicationUser user, IList<string> roles)
    {
        // Claims are pieces of information stored inside the token.
        // The mobile app/backend can later read these claims.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("fullName", user.FullName),
            new("roleType", user.RoleType)
        };

        // Add Identity roles into the token.
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Convert secret key string into bytes.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        // Signing credentials prove the token was created by our API.
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}