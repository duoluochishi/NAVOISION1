using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.NP.Tools.DataTransfer.Model
{
    public interface IExport
    {
        public long EstimateTotalFileSize { get; }
        Task<(bool result, string msg)> ExportAsync(string targetPath, CancellationToken cancellationToken);
    }
}
