using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeriesModel = NV.CT.ImageViewer.Model.SeriesModel;

namespace NV.CT.ImageViewer.Extensions
{
    public class UpdateSelectedSeriesModel:PubSubEvent<SeriesModel>
    {
    }
}
