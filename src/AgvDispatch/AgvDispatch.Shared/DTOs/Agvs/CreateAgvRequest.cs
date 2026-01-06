using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.DTOs.Agvs
{
    /// <summary>
    /// 创建 AGV 小车请求 DTO
    /// </summary>
    public class CreateAgvRequest
    {
        public string AgvCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int SortNo { get; set; }
        public string? Description { get; set; }
    }
}
