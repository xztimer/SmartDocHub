using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.Reponse;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartDocHub.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public LoginController(SignInManager<User> signInManager, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _configuration = configuration;
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var res = await _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false);

            if (res.Succeeded)
            {
                var token = GenerateToken(loginDto.UserName);
                return Ok(new { Token = token, Message = "登陆成功" });
            }
            else
            {
                var responseResult = new ResponseResultDto();
                responseResult.SetError("账号或密码错误");
                return BadRequest(responseResult);
            }

        }

        private string GenerateToken(string userName)
        {
            var jwtSection = _configuration.GetSection("Authentication").GetSection("JwtBearer");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecurityKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userName)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
