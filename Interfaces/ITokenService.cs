using BatoClinic.Api.Entities;

namespace BatoClinic.Api.Interfaces;

// Token service creates access tokens and refresh tokens.
public interface ITokenService
{
    string CreateToken(ApplicationUser user, IList<string> roles);

    string CreateRefreshToken();

    string HashToken(string token);
}