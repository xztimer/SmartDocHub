using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.AuditLog;
using SmartDocHub.Web.Captcha;
using SmartDocHub.Web.Reponse;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartDocHub.Web.Controllers
{
    /// <summary>
    /// 登录控制器
    /// </summary>
    /// <param name="signInManager"></param>
    /// <param name="configuration"></param>
    /// <param name="memoryCache"></param>
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(SignInManager<User> signInManager,
        UserManager<User> userManager,
        IConfiguration configuration, 
        IMemoryCache memoryCache) : ControllerBase
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [AuditLog(IsOpen = false)]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            //if (string.IsNullOrEmpty(loginDto.CodeKey))
            //{
            //    return BadRequest("验证码 Key 不能为空！");
            //}
            //else
            //{
            //    var code = memoryCache.Get(loginDto.CodeKey);
            //    if (code == null || !loginDto.Code.ToLower().Equals(code.ToString().ToLower()))
            //    {
            //        return BadRequest("验证码错误或已过期！");
            //    }
            //}

            //memoryCache.Remove(loginDto.CodeKey);
            var user = await signInManager.UserManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                var responseResult = new ResponseResultDto();
                responseResult.SetError("账号或密码错误");
                return BadRequest(responseResult);
            }

            var res = await signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

            if (!res.Succeeded)
            {
                var responseResult = new ResponseResultDto();
                responseResult.SetError("账号或密码错误");
                return BadRequest(responseResult);

            }

            var token = await GenerateToken(user);
            user.LastLoginTime = DateTime.UtcNow;
            await signInManager.UserManager.UpdateAsync(user);
            return Ok(new 
            { 
                Token = token.accessToken, 
                RefreshToken = token.refreshToken, 
                Message = "登陆成功" 
            });
        }

        private async Task<(string accessToken, string refreshToken)> GenerateToken(User user)
        {
            var jwtSection = configuration.GetSection("Authentication").GetSection("JwtBearer");
            var userRoles = await userManager.GetRolesAsync(user);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecurityKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var accessClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())                
            };
            foreach(var role in userRoles)
            {
                accessClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var accessToken = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: accessClaims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: credentials
            );

            var refreshClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, "RefreshToken")
            };

            var refreshToken = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: refreshClaims,
                expires: DateTime.UtcNow.AddMinutes(600),
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(accessToken), new JwtSecurityTokenHandler().WriteToken(refreshToken));
        }

        /// <summary>
        /// 验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("code")]
        [AuditLog(IsOpen = false)]
        public IActionResult Code()
        {
            //var code = CaptchaGenerator.CreateValidCode(4);
            var code = "1234";
            var buffer = CaptchaGenerator.GenerateCode(code, 100, 30);

            var codeKey = Guid.NewGuid().ToString();
            memoryCache.Set(codeKey, code, DateTimeOffset.Now.AddMinutes(5));

            return Ok(new
            {
                codeKey,
                image = "data:image/png;base64," + Convert.ToBase64String(buffer)
            });

        }

        [HttpPost("refresh")]
        public IActionResult RefreshToken()
        {
            return Ok();
        }
    }
}
