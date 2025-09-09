using System;

namespace NV.CT.Service.HardwareTest.Attachments.EventArguments
{
    public class AdjustWindowEventArgs : EventArgs
    {
        public uint Width {set; get;}
        public uint Height { set; get; }
    }
}
