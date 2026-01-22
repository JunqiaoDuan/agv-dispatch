using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.DTOs.BackgroundJobLogs
{
    /// <summary>
    /// 后台任务日志分页查询结果
    /// </summary>
    public class BackgroundJobLogPagedResult
    {
        public List<BackgroundJobLogDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
