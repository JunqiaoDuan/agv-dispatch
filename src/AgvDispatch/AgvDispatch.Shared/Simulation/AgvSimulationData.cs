using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.Simulation
{
    /// <summary>
    /// AGV 模拟数据（用于渲染）
    /// </summary>
    public class AgvSimulationData
    {
        /// <summary>
        /// AGV 当前位置
        /// </summary>
        public AgvPosition Position { get; set; } = new();

        /// <summary>
        /// 起点节点ID
        /// </summary>
        public Guid StartNodeId { get; set; }

        /// <summary>
        /// 终点节点ID
        /// </summary>
        public Guid EndNodeId { get; set; }

        /// <summary>
        /// 路径的边ID列表
        /// </summary>
        public List<Guid> PathEdgeIds { get; set; } = [];

        /// <summary>
        /// 配置信息
        /// </summary>
        public AgvSimulationConfig Config { get; set; } = new();
    }
}
