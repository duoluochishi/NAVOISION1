using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.DAL.Dtos
{
    [Serializable]
    public class WarmUpConfigDto
    {
        public WarmUpConfigDto()
        {
            this.MaxWarmupCount = 20;
        }

        public int MaxWarmupCount { get; set; }
    }
}