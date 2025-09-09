using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer
{
    public class ToDataProfile:Profile
    {
        public ToDataProfile()
        {
            CreateMap<ScreenshotProperties, ScreenshotParameters>().ReverseMap();
        }
    }
        
}
