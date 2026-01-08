using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.DTOs.Auth
{
    /// <summary>
    /// 登录响应
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT Token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInfoDto User { get; set; } = new();
    }
}
