using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageModel = NV.CT.ImageViewer.Model.ImageModel;
namespace NV.CT.ImageViewer.Extensions
{
    public class UpdateSelectedImageModel:PubSubEvent<ImageModel>
    {
    }
}
