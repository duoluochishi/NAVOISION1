using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public class HelicalScanHandler : AbstractScanHandler
    {
        public HelicalScanHandler(
            ILogService logService,
            IMessagePrintService messagePrintService,
            CancellationTokenSource cancellationTokenSource)
            : base(logService, messagePrintService, cancellationTokenSource)
        {
        }

        protected override Task<CommandResult> Scan()
        {
            throw new NotImplementedException();
        }
    }
}
