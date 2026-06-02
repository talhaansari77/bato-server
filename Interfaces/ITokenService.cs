using BatoClinic.Api.Entities;

namespace BatoClinic.Api.Interfaces;

// Interface = contract.
// It says any TokenService must have a CreateToken method.
public interface ITokenService
{
    string CreateToken(ApplicationUser user, IList<string> roles);
}