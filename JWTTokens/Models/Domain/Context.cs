using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JWTTokens.Models.Domain
{
    public class Context(DbContextOptions<Context> options) : IdentityDbContext<AppUser>(options)
    {

        public DbSet<TokenInfo> TokenInfos { get; set; }
    }
}
