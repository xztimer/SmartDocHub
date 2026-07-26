using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocHub.Service.UserApp.Dto
{
    public class UserAllDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }

        public string? NickName { get; set; }
    }
}
