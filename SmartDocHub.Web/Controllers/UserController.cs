using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

using SmartDocHub.Service.Exceptions;
using SmartDocHub.Service.UserApp;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.Reponse;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartDocHub.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;

        public UserController(IUserService userService, IConfiguration configuration,IMemoryCache memoryCache)
        {
            _userService = userService;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.UserName) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest(new ApiResult<object>(ResponseCode.BusinessError, "用户名或密码不能为空"));
            }
            var res = await _userService.CheckLogin(loginDto);

            var token = GenerateToken(res.Id, res.UserName);

            return Ok(new ApiResult<string>(ResponseCode.Success,"",token));
        }


        private string GenerateToken(long userId, string userName)
        {
            var jwtSection = _configuration.GetSection("Authentication").GetSection("JwtBearer");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecurityKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName)
        };


            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
