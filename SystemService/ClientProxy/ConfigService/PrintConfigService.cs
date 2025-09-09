using NV.MPS.Communication;
using NV.CT.ConfigService.Contract;
using NV.CT.CTS.Models;
using NV.CT.CTS.Extensions;
using NV.CT.ConfigService.Contract.Models;

namespace NV.CT.ClientProxy.ConfigService
{
    public class PrintConfigService : IPrintConfigService
    {
        public event EventHandler<PrintingImageUpdateInfo> PrintImagesUpdated;

        private readonly PrintConfigClientProxy _clientProxy;
        public PrintConfigService(PrintConfigClientProxy clientProxy)
        {
            _clientProxy = clientProxy;
        }

        public bool AcceptAppendingImages(string studyId, List<PrintingImageProperty> images)
        {
            var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IPrintConfigService).Namespace,
                SourceType = nameof(IPrintConfigService),
                ActionName = nameof(IPrintConfigService.AcceptAppendingImages),
                Data = Tuple.Create(studyId, images).ToJson()
            });
            if (commandResponse.Success)
            {
                return Convert.ToBoolean(commandResponse.Data);
            }

            return false;
        }        

    }
}
