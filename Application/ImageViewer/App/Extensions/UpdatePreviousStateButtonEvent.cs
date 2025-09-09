using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV.CT.ImageViewer.Extensions
{
    public class Update2DPreviousStateButtonEvent:PubSubEvent<bool>
    {
    }
    public class Update3DPreviousStateButtonEvent : PubSubEvent<TextBlockListType>
    {
    }
}
