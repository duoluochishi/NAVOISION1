using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DmsTest.Model
{
   
    public enum CalcStatus : byte
    {
        OK = 1,
        POK,
        NG,
    }
   
    public partial class ModuleTestStatus
    {
       
        public int ChannelNo { get; set; }

        public CalcStatus CalcStatus { get; set; }
    }

}
