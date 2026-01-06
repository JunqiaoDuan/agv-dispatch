using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgvDispatch.Shared.DTOs.Agvs
{
    /// <summary>
    /// 分页请求
    /// </summary>
    public class PagedAgvRequest
    {
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public string? SearchText { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}
