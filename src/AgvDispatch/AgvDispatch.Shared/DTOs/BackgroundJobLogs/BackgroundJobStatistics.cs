using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.DTOs.BackgroundJobLogs
{
    /// <summary>
    /// 后台任务统计信息
    /// </summary>
    public class BackgroundJobStatistics
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<JobTypeCount> ByJobType { get; set; } = new();
    }
}
