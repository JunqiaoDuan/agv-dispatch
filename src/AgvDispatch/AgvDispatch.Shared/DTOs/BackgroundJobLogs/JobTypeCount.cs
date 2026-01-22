using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.DTOs.BackgroundJobLogs
{
    /// <summary>
    /// 按任务类型统计
    /// </summary>
    public class JobTypeCount
    {
        public string JobName { get; set; } = string.Empty;
        public string JobDisplayName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
