using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace StreamingAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var dbUser = _config["Admin:Username"];
            var dbPass = _config["Admin:Password"];

            if (request.Username == dbUser && request.Password == dbPass)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sa2951sa_S_bu_sifrenin_en_az_32_karakter_olmasi_lazim_123"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "StreamAdmin",
                    audience: "StreamAdmin",
                    expires: DateTime.Now.AddMinutes(1),
                    signingCredentials: creds
                );

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            return Unauthorized(new { message = "Invalid username or password!" });
        }

        [Authorize]
        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sa2951sa_S_bu_sifrenin_en_az_32_karakter_olmasi_lazim_123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "StreamAdmin",
                audience: "StreamAdmin",
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: creds
            );
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }

    public class LoginRequest
    {
        [JsonPropertyName("Username")]
        public string? Username { get; set; }
        [JsonPropertyName("Password")]
        public string? Password { get; set; }
    }
}