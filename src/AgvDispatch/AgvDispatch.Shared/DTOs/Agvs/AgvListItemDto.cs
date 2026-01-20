using AgvDispatch.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.DTOs.Agvs
{
    /// <summary>
    /// AGV 小车列表项 DTO
    /// </summary>
    public class AgvListItemDto
    {
        public Guid Id { get; set; }
        public string AgvCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public AgvStatus AgvStatus { get; set; }
        public int Battery { get; set; }
        public decimal BatteryVoltage { get; set; }
        public decimal Speed { get; set; }
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public decimal PositionAngle { get; set; }
        public bool HasCargo { get; set; }
        public string? ErrorCode { get; set; }
        public DateTimeOffset? LastOnlineTime { get; set; }
        public Guid? CurrentStationId { get; set; }
        public Guid? CurrentTaskId { get; set; }
        public int SortNo { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? CreationDate { get; set; }
    }
}
