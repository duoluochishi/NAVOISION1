using NV.CT.ConfigService.Contract.Models;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using System.Collections.Generic;

namespace NV.CT.Print
{
    public class PrintConfigService : IPrintConfigService
    {
        private readonly ILogger<PrintConfigService>? _logger;
        private readonly IPrintConfigManager _printConfigManager;

        public event EventHandler<PrintingImageUpdateInfo> PrintImagesUpdated;

        public PrintConfigService(ILogger<PrintConfigService>? logger, IPrintConfigManager printConfigManager)
        { 
            _logger = logger;
            _printConfigManager = printConfigManager;
        }

        public bool AcceptAppendingImages(string studyId, List<PrintingImageProperty> imageList)
        {
            _logger?.LogDebug($"receive AcceptAppendingImages for studyId:{studyId}");
            var result = this._printConfigManager.AppendImagesToPrint(studyId, imageList);
            if (result)
            {
                var updateInfo = new PrintingImageUpdateInfo() {
                    StudyId = studyId,
                    PrintingImageList = imageList,
                };

                PrintImagesUpdated?.Invoke(this, updateInfo);
            }
            return result;
        }

    }
}
