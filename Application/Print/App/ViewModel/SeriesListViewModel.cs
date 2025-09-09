//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.Print.Events;
using NV.CT.Print.Models;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NV.CT.Print.ViewModel
{
    public class SeriesListViewModel : BaseViewModel
    {
        private readonly ILogger<SeriesListViewModel>? _logger;        

        private PrintingStudyModel? _currentPrintingStudy;
        public PrintingStudyModel? CurrentPrintingStudy
        {
            get => _currentPrintingStudy;
            set => SetProperty(ref _currentPrintingStudy, value);
        }

        private List<PrintingImageModel>? _imageModels;
        public List<PrintingImageModel>? ImageModels
        {
            get => _imageModels;
            set => SetProperty(ref _imageModels, value);
        }

        private PrintingImageModel? _selectedImageModel;
        public PrintingImageModel? SelectedImageModel
        {
            get => _selectedImageModel;
            set => SetProperty(ref _selectedImageModel, value);
        }

        private bool _isShowImageChecked = true;
        public bool IsShowImageChecked
        {
            get => _isShowImageChecked;
            set
            {
                SetProperty(ref _isShowImageChecked, value);
            } 
        }

        public SeriesListViewModel(ILogger<SeriesListViewModel> logger)
        {
            _logger = logger;
            Commands.Add(PrintConstants.COMMAND_IMAGE_VIEW_MOUSE_DOUBLE, new DelegateCommand(OnImageViewMouseDoubleClick));

            EventAggregator.Instance.GetEvent<SelectedStudyChangedEvent>().Subscribe(OnCurrentStudyChanged);
        }

        private void OnCurrentStudyChanged(PrintingStudyModel printingStudyModel)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                CurrentPrintingStudy = printingStudyModel;

                if (printingStudyModel is null || printingStudyModel.PrintingSeriesModelList is null)
                {
                    _logger?.LogDebug("printingStudyModel in OnCurrentStudyChanged is null.");
                    return;
                }

                var sourceSeriesModels = printingStudyModel.PrintingSeriesModelList.Where(s => s.SeriesType == Constants.SERIES_TYPE_IMAGE || s.SeriesType == Constants.SERIES_TYPE_DOSE_REPORT).ToArray();

                List<PrintingImageModel> imageModels = new List<PrintingImageModel>();

                int count = sourceSeriesModels.Length;
                for (int i = 0; i < count; i++)
                {
                    var sourceSeriesModel = sourceSeriesModels[i];
                    var printingImageModel = new PrintingImageModel();
                    printingImageModel.Number = i + 1; //设置序号显示在列表中
                    printingImageModel.SeriesId = sourceSeriesModel.Id;
                    printingImageModel.SeriesPath = sourceSeriesModel.SeriesPath;
                    printingImageModel.Description = sourceSeriesModel.SeriesDescription;
                    printingImageModel.ImageSource = GetThumbImage(printingImageModel.SeriesPath);
                    imageModels.Add(printingImageModel);
                }
                ImageModels = imageModels;
                if (ImageModels.Count > 0)
                {
                    SelectedImageModel = ImageModels[0];
                }
                else
                {
                    SelectedImageModel = null;
                }
            });
        }

        private WriteableBitmap? GetThumbImage(string path)
        {
            string defaultFileFullPath = string.Empty;
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                defaultFileFullPath = fileInfo.FullName;
            }
            else if (Directory.Exists(path))
            {
                var fileInfo = Directory.GetFiles(path, "*.dcm").Select(n => new FileInfo(n)).FirstOrDefault();
                if (fileInfo is null)
                {
                    _logger?.LogDebug($"No dcm file found under path: {path}");
                    return null;
                }
                defaultFileFullPath = fileInfo.FullName;
            }
            else
            {
                string errorMessage = $"The path does not exist:{path}";
                _logger?.LogDebug(errorMessage);

                return null;
                //throw new DirectoryNotFoundException(errorMessage);
            }

            return NV.CT.DicomUtility.DicomImage.DicomImageHelper.Instance.GenerateThumbImage(defaultFileFullPath, 75, 75);
        }
        
        private void OnImageViewMouseDoubleClick()
        {
            if (CurrentPrintingStudy is null)
            {
                _logger?.LogInformation("CurrentPrintingStudy of OnImageViewMouseDoubleClick is null.");
                return;
            }

            if (SelectedImageModel is null)
            {
                _logger?.LogInformation("SelectedImageModel of OnImageViewMouseDoubleClick is null.");
                return;              
            }
            _logger?.LogInformation("sllin OnImageViewMouseDoubleClick sllin 151.");
            var selectedSeries = CurrentPrintingStudy?.PrintingSeriesModelList?.FirstOrDefault( s=> s.Id == SelectedImageModel.SeriesId);
            _logger?.LogInformation("sllin OnImageViewMouseDoubleClick sllin 153.");
            if (selectedSeries is null)
            {
                _logger?.LogInformation("No selectedSeries found in PrintingSeriesModelList.");
                return;
            }
            _logger?.LogInformation("sllin OnImageViewMouseDoubleClick sllin 159.");
            EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Publish(selectedSeries);
            _logger?.LogInformation("sllin OnImageViewMouseDoubleClick sllin 161.");
        }

    }
}
