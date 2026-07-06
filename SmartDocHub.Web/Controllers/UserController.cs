using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.UserApp;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.Reponse;

namespace SmartDocHub.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(IUserService userService,
            UserManager<User> userManager,
            IConfiguration configuration,
            IMemoryCache memoryCache,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _userManager = userManager;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] UserCreateDto userCreateDto)
        {
            var user = _mapper.Map<User>(userCreateDto);
            var userName = _httpContextAccessor.HttpContext.User.Identity.Name;

            user.EmailConfirmed = true;
            user.SecurityStamp = DateTime.UtcNow.Ticks.ToString();
            user.NormalizedUserName = userName;

            PasswordHasher<User> ph = new PasswordHasher<User>();
            user.PasswordHash = ph.HashPassword(user, userCreateDto.Password);

            var res = await _userManager.CreateAsync(user);
            if (res.Succeeded)
            {
                return Created(string.Empty, user);
            }
            else
            {
                var responseResult = new ResponseResultDto();
                responseResult.SetError("请检查用户账号，是否重复！");
                return BadRequest(responseResult);
            }
        }



    }
}
