using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.QualityTest.Enums
{
    public enum ImageFuncType
    {
        RawView,
        CutView,
        CorrView,
        SinogramView,
        ReconView,
        None,
        OneToOne,
        Fit,
        Zoom,
        Pan,
        Rect,
        Angle,
        Ellipse,
        Delete,
    }
}