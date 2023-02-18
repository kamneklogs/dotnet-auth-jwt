using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using e09.Services.Interfaces;
using e09.Models;

namespace e09.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUser user)
        {
            if (user is null)
            {
                return BadRequest("Invalid client request");
            }

            if (!_userService.UserExistsByEmailAsync(user.Email).Result)
            {
                return NotFound("User not found");
            }

            if (_userService.ValidatePasswordAsync(user.Email, user.Password).Result)
            {
                var userSigned = _userService.GetUserByEmailAsync(user.Email).Result;

                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345")); // We could use a security service to unify this work and apply dry principle
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: "https://localhost:5001",
                    audience: "https://localhost:5001",
                    claims: new List<Claim>(
                        new Claim[] {
                            new Claim(ClaimTypes.Name, userSigned.Name),
                            new Claim(ClaimTypes.Email, userSigned.Email),
                            new Claim(ClaimTypes.Role, userSigned.Role.ToString()),
                            new Claim("IsActiveRole" , userSigned.IsActiveRole.ToString())
                        }
                    ),
                    /*
                    Decoded jwt payload example:
                    {
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "John Doe",
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "john@test.com",
                        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin",
                        "IsActiveRole": "True",
                        "exp": 1676239937,
                        "iss": "https://localhost:5001",
                        "aud": "https://localhost:5001"
                    }

                    Question: Using ClaimTypes we could give information about the platform used to generate the token (microsoft dotnet), is this a security problem or some attack vector?
                    */
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: signinCredentials
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return Ok(new AuthenticatedResponse { Token = tokenString });
            }
            return Unauthorized("Invalid credentials");
        }
    }
}