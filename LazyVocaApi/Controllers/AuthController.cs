using LazyVocaApi.Entities;
using LazyVocaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LazyVocaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _usersService;
        private readonly IConfiguration _configuration;

        public AuthController(
            IConfiguration configuration,
            IUserService usersService)
        {
            _usersService = usersService;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost()]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var loggedUser = await _usersService.GetAsync(request.UserName, request.Password);

            if (loggedUser != null)
            {
                var token = GetToken(loggedUser.Id);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    tokenType = "Bearer",
                    expiresIn = TimeSpan.FromDays(30).TotalSeconds,
                    User = loggedUser
                });
            }

            return Unauthorized();
        }

        [HttpPut]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequest newUser)
        {
            if (string.IsNullOrEmpty(newUser.UserName) || string.IsNullOrEmpty(newUser.Password))
            {
                return BadRequest();
            }

            if (await _usersService.CheckDuplicateAsync(newUser.UserName))
            {
                return Conflict();
            }

            await _usersService.CreateAsync(newUser.UserName, newUser.Password);

            return NoContent();
        }

        private JwtSecurityToken GetToken(string name)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, name) };

            var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["SecurityKey"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "LazyVocaApi",
                audience: "LazyVocaApi",
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds);

            return token;
        }

    }
}
