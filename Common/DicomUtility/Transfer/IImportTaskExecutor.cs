using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DicomUtility.Transfer
{
    public interface IImportTaskExecutor
    {
        string PatientNameListString { get;  }

        event EventHandler<ExecuteStatusInfo> ExecuteStatusChanged;

        void Start();
    }
}
