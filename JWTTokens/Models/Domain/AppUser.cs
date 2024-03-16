using Microsoft.AspNetCore.Identity;

namespace JWTTokens.Models.Domain
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
