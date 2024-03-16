using JWTTokens.Models.Domain;
using JWTTokens.Models.DTO;
using JWTTokens.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace JWTTokens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController(Context context, ITokenService tokenService) : ControllerBase
    {
        private readonly Context _context = context ?? throw new ArgumentNullException(nameof(context)); // Context object to use for database connection
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService)); // Service to be used for token creation and management

        // Endpoint refreshing AccessToken
        [HttpPost]
        public IActionResult Refresh(RefreshTokenRequest tokenApiModel)
        {
            if (tokenApiModel is null)
                return BadRequest("Invalid client request");

            // AccessToken and RefreshToken values are retrieved
            string accessToken = tokenApiModel.AccessToken;
            string refreshToken = tokenApiModel.RefreshToken;

            // User information from AccessToken is retrieved
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userName = principal.Identity.Name;

            // User is checked from database
            var user = _context.TokenInfos.SingleOrDefault(x => x.UserName == userName);

            // An error is returned if the user cannot be found or the Refresh Token is invalid or expired
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.Now)
                return BadRequest("Invalid client request");

            // New Access Token and Refresh Token are created
            var newAccessToken = _tokenService.GetToken(principal.Claims);
            var newRefreshToken = _tokenService.GetRefreshToken();

            // The new RefreshToken is replaced with the old RefreshToken and the database is updated
            user.RefreshToken = newRefreshToken;
            _context.SaveChanges();

            //New Access Token and Refresh Token are returned as response
            return Ok(new RefreshTokenRequest()
            {
                AccessToken = newAccessToken.TokenString,
                RefreshToken = newRefreshToken
            });
        }

        // Endpoint invalidating AccessToken
        [HttpPost, Authorize]
        public IActionResult Revoke()
        {
            try
            {
                var username = User.Identity.Name;
                var user = _context.TokenInfos.SingleOrDefault(x => x.UserName == username);

                if (user is null)
                    return BadRequest("Invalid user");

                user.RefreshToken = null;
                _context.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
