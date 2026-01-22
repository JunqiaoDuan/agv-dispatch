using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.Enums
{
    /// <summary>
    /// 任务执行结果
    /// </summary>
    public enum JobExecutionResult
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 1,

        /// <summary>
        /// 失败
        /// </summary>
        Failed = 2,

        /// <summary>
        /// 跳过（无操作）
        /// </summary>
        Skipped = 3
    }
}
