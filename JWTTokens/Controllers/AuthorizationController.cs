using JWTTokens.Models;
using JWTTokens.Models.Domain;
using JWTTokens.Models.DTO;
using JWTTokens.Repositories.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JWTTokens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController(Context context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService) : ControllerBase
    {
        private readonly Context _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly UserManager<AppUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly RoleManager<IdentityRole> _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await GetUserByUsername(loginModel.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                return Unauthorized();
            }

            var authClaims = await GetUserClaims(user);
            var token = _tokenService.GetToken(authClaims);
            var refreshToken = _tokenService.GetRefreshToken();
            await UpdateRefreshToken(user.UserName, refreshToken);

            return Ok(new LoginResponse
            {
                Name = user.Name,
                Username = user.UserName,
                Token = token.TokenString,
                RefreshToken = refreshToken,
                ExpirationDate = token.ValidTo,
                StatusCode = 1,
                Message = "Logged in"
            });
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> Registration([FromBody] RegistrationModel model)
        {
            return await RegisterUser(model, AppRoles.User);
        }

        [HttpPost("RegistrationAdmin")]
        public async Task<IActionResult> RegistrationAdmin([FromBody] RegistrationModel model)
        {
            return await RegisterUser(model, AppRoles.Admin);
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            var user = await GetUserByUsername(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
            {
                return Unauthorized();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to change password");
            }

            return Ok("Password changed successfully");
        }

        private async Task<AppUser> GetUserByUsername(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        private async Task<List<Claim>> GetUserClaims(AppUser user)
        {
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            return authClaims;
        }

        private async Task UpdateRefreshToken(string username, string refreshToken)
        {
            var tokenInfo = _context.TokenInfos.FirstOrDefault(x => x.UserName == username);
            if (tokenInfo == null)
            {
                tokenInfo = new TokenInfo
                {
                    UserName = username,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = DateTime.Now.AddDays(7)
                };
                _context.TokenInfos.Add(tokenInfo);
            }
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.RefreshTokenExpiry = DateTime.Now.AddDays(7);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<IActionResult> RegisterUser(RegistrationModel model, string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please pass all required fields");
            }

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return BadRequest("Invalid username");
            }

            var user = new AppUser
            {
                UserName = model.Username,
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = model.Email,
                Name = model.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest("User creation failed");
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            if (await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return Ok("User successfully registered");
        }
    }
}
