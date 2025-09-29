using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.CTS.Enums
{
    public enum TaskStatus
    {
        Created, 
        Waiting, 
        Executing, 
        Cancelled, 
        Finished, 
        Error
    }
}
