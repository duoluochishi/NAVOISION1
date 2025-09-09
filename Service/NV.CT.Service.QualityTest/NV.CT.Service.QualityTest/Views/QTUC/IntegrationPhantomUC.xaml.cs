using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Alg;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Extension;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;
using NV.CT.Service.QualityTest.Services;
using NV.CT.Service.QualityTest.Utilities;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using NV.MPS.ImageIO;

namespace NV.CT.Service.QualityTest.Views.QTUC
{
    /// <summary>
    /// PhantomUC.xaml 的交互逻辑
    /// </summary>
    public partial class IntegrationPhantomUC
    {
        private readonly IMessagePrintService _printLog;
        private readonly IIntegrationPhantomService _integrationPhantomService;

        public IntegrationPhantomUC(IMessagePrintService printLog, IIntegrationPhantomService integrationPhantomService) : base(1)
        {
            InitializeComponent();
            _printLog = printLog;
            _integrationPhantomService = integrationPhantomService;
            SetImageControlBorders(BorderIC1);
            AddDataGridColumns(DataGridParas);
        }

        public override bool CommandCanExecute(ImageFuncType type)
        {
            return type switch
            {
                ImageFuncType.RawView => false,
                ImageFuncType.CutView => false,
                ImageFuncType.CorrView => false,
                ImageFuncType.SinogramView => false,
                ImageFuncType.ReconView => false,
                _ => true
            };
        }

        public override ResultModel SetScanAndReconParam(ItemEntryModel model)
        {
            var position = _integrationPhantomService.GetLocatePosition();
            model.SetScanAndReconParam(position);
            return ResultModel.Create(true);
        }

        public override ResultModel BeforeScan(ItemEntryModel param)
        {
            var ic = GetImageControl(param, 0);
            ic.ClearImages();
            return ResultModel.Create(true);
        }

        public override async Task<ResultModel> AfterRecon(ItemEntryModel param)
        {
            var imageWidth = 0;
            var imageHeight = 0;
            var files = Directory.GetFiles(param.OfflineReconImageFolder).OrderBy(i => i).ToArray();
            var ic = GetImageControl(param, 0);
            var imageDataList = new List<ImageData>();

            for (var i = 0; i < files.Length; i++)
            {
                if (i == 0)
                {
                    var imageData = ImageControlUtility.CreateImageData(files[i]);
                    imageDataList.Add(imageData);
                    imageWidth = imageData.ImageWidth;
                    imageHeight = imageData.ImageHeight;
                }
                else
                {
                    imageDataList.Add(new ImageData() { FilePath = files[i] });
                }
            }

            ic.AddImage(imageDataList);
            await Task.Delay(100);

            if (ic.LastImageControl != null)
            {
                ic.LastImageControl.ContextMenu = null;
            }

            if (param.Param is not IntegrationPhantomParamModel model)
            {
                _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Image_ModelTypeError, param.GetType().Name));
                return ResultModel.Create(false, "Param Type Error");
            }

            var ballsRes = AlgMethods.GetPhantomLocateBalls(param.OfflineReconImageFolder, ConfigFolder);

            if (!ballsRes.Success)
            {
                var errorMsg = ballsRes.Code.GetErrorCodeDescription();
                model.Value.ErrorMsg = errorMsg;
                param.Result = false;
                _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Alg_InvokeError, errorMsg));
                return ResultModel.Create(false, "Alg Invoke Error");
            }

            model.Value.ImageWidth = imageWidth;
            model.Value.ImageHeight = imageHeight;
            model.Value.Balls[0] = ballsRes.Result.BallTop;
            model.Value.Balls[1] = ballsRes.Result.BallBottom;
            model.Value.Balls[2] = ballsRes.Result.BallLeft;
            model.Value.Balls[3] = ballsRes.Result.BallRight;
            model.Validate();
            return ResultModel.Create(true);
        }
    }
}