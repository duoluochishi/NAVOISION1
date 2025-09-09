using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer.Extensions
{
    public class Update2DSwitchButtonEvent:PubSubEvent<string>
    {
    }
    public class Update3DSwitchButtonEvent : PubSubEvent<string>
    {
    }
    public class Update2DHotKeyEvent : PubSubEvent<string>
    {
    }
    public class Update3DHotKeyEvent : PubSubEvent<string>
    {
    }
}
