using System;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class AppUser
    {
        public UserInfo Account { get; set; }

        public string Token { get; set; }

        public DateTime Expires { get; set; }

        public string RefreshToken { get; set; }
    }
}