using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.DTOs.Agvs
{
    /// <summary>
    /// 更新 AGV 小车请求 DTO
    /// </summary>
    public class UpdateAgvRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
        public int SortNo { get; set; }
        public string? Description { get; set; }
    }
}
