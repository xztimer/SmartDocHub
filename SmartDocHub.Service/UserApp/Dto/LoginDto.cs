using System.ComponentModel.DataAnnotations;

namespace SmartDocHub.Service.UserApp.Dto;

public class LoginDto
{
    [Required]
    [StringLength(20,MinimumLength =5)]
    public string UserName { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 6)]
    public string Password { get; set; }
    [Required]
    public string CodeKey { get; set; }
    [Required]
    public string Code { get; set; }
}