using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.DAL.Dtos
{
    [Serializable]
    public class WarmUpTaskDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 休息时间
        /// </summary>
        public int RestTimeInterval { get; set; }
        public int KV { get; set; }
        public int MA { get; set; }
        public int ScanTimes { get; set; }
    }
}
