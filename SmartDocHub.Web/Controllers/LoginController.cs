using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.AuditLog;
using SmartDocHub.Web.Auth;
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
    /// <param name="userManager"></param>
    /// <param name="configuration"></param>
    /// <param name="memoryCache"></param>
    /// <param name="tokenStatusValidator"></param>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LoginController(SignInManager<User> signInManager,
        UserManager<User> userManager,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        ITokenStatusValidator tokenStatusValidator) : ControllerBase
    {
        private const string CaptchaCachePrefix = "login:captcha:";

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [AuditLog(IsOpen = false)]
        [EnableRateLimiting("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.CodeKey))
            {
                return BadRequest("验证码 Key 不能为空！");
            }

            var cacheKey = CaptchaCachePrefix + loginDto.CodeKey;
            var code = memoryCache.Get<string>(cacheKey);
            memoryCache.Remove(cacheKey);

            if (code == null || !string.Equals(loginDto.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("验证码错误或已过期！");
            }

            var user = await userManager.FindByNameAsync(loginDto.UserName);
            if (user == null || user.IsDeleted)
            {
                var notFoundResult = new ResponseResultDto();
                notFoundResult.SetError("账号或密码错误");
                return BadRequest(notFoundResult);
            }

            // 使用 CheckPasswordSignInAsync 而非 PasswordSignInAsync：只校验口令，不写入 Cookie
            var res = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

            if (res.IsLockedOut)
            {
                var lockedResult = new ResponseResultDto();
                lockedResult.SetError("账号已被锁定，请稍后再试");
                return BadRequest(lockedResult);
            }

            if (!res.Succeeded)
            {
                var failedResult = new ResponseResultDto();
                failedResult.SetError("账号或密码错误");
                return BadRequest(failedResult);
            }

            var token = await GenerateToken(user);
            user.LastLoginTime = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            return Ok(new
            {
                Token = token.accessToken,
                RefreshToken = token.refreshToken,
                Message = "登陆成功"
            });
        }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AuditLog(IsOpen = false)]
        [EnableRateLimiting("login")]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
            {
                return Unauthorized(BuildError("缺少刷新令牌"));
            }

            var principal = ValidateTokenIgnoringLifetime(request.RefreshToken);
            if (principal is null
                || !string.Equals(principal.FindFirstValue(JwtClaimNames.TokenType), JwtClaimNames.RefreshToken, StringComparison.Ordinal))
            {
                return Unauthorized(BuildError("刷新令牌无效或已过期"));
            }

            var userIdText = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdText, out _))
            {
                return Unauthorized(BuildError("刷新令牌无效或已过期"));
            }

            var user = await userManager.FindByIdAsync(userIdText);
            if (user is null || user.IsDeleted || user.Status != UserStatus.Normal)
            {
                return Unauthorized(BuildError("账号不可用，请重新登录"));
            }

            // 安全戳已变更（改密、停用、登出）则拒绝刷新
            var tokenStamp = principal.FindFirstValue(JwtClaimNames.SecurityStamp) ?? string.Empty;
            if (!string.Equals(tokenStamp, user.SecurityStamp ?? string.Empty, StringComparison.Ordinal))
            {
                return Unauthorized(BuildError("登录状态已失效，请重新登录"));
            }

            var token = await GenerateToken(user);
            return Ok(new
            {
                Token = token.accessToken,
                RefreshToken = token.refreshToken,
                Message = "刷新成功"
            });
        }

        /// <summary>
        /// 登出：变更安全戳，使该账号已签发的全部令牌立即失效
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdText, out var userId))
            {
                return NoContent();
            }

            var user = await userManager.FindByIdAsync(userIdText);
            if (user is not null)
            {
                await userManager.UpdateSecurityStampAsync(user);
            }

            tokenStatusValidator.Invalidate(userId);
            return NoContent();
        }

        /// <summary>
        /// 验证码
        /// </summary>
        /// <returns></returns>
        [AuditLog(IsOpen = false)]
        [EnableRateLimiting("login")]
        [HttpGet("code")]
        public IActionResult Code()
        {
            var code = CaptchaGenerator.CreateValidCode(4);
            var buffer = CaptchaGenerator.GenerateCode(code, 100, 30);

            var codeKey = Guid.NewGuid().ToString("N");
            memoryCache.Set(CaptchaCachePrefix + codeKey, code, TimeSpan.FromMinutes(5));

            return Ok(new
            {
                codeKey,
                image = "data:image/png;base64," + Convert.ToBase64String(buffer)
            });
        }

        private async Task<(string accessToken, string refreshToken)> GenerateToken(User user)
        {
            var jwtSection = configuration.GetSection("Authentication").GetSection("JwtBearer");
            var userRoles = await userManager.GetRolesAsync(user);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecurityKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var accessClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(JwtClaimNames.TokenType, JwtClaimNames.AccessToken),
                new(JwtClaimNames.SecurityStamp, user.SecurityStamp ?? string.Empty)
            };
            foreach (var role in userRoles)
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
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(JwtClaimNames.TokenType, JwtClaimNames.RefreshToken),
                new(JwtClaimNames.SecurityStamp, user.SecurityStamp ?? string.Empty)
            };

            var refreshToken = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: refreshClaims,
                expires: DateTime.UtcNow.AddMinutes(600),
                signingCredentials: credentials
            );

            var handler = new JwtSecurityTokenHandler();
            return (handler.WriteToken(accessToken), handler.WriteToken(refreshToken));
        }

        /// <summary>
        /// 校验令牌签名与签发方（忽略有效期），用于刷新场景。
        /// </summary>
        private ClaimsPrincipal? ValidateTokenIgnoringLifetime(string token)
        {
            var jwtSection = configuration.GetSection("Authentication").GetSection("JwtBearer");
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSection["Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecurityKey"]!)),
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        private static ResponseResultDto BuildError(string message)
        {
            var result = new ResponseResultDto();
            result.SetError(message);
            return result;
        }
    }
}
