using NV.CT.ImageViewer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer.ViewModel
{
    public class BatchReconViewModel: BaseViewModel
    {
        private readonly Image3DViewModel? _vm3d;
        private ObservableCollection<KeyValuePair<int, string>> _viewOrientationList = new();
        public ObservableCollection<KeyValuePair<int, string>> ViewOrientationList
        {
            get => _viewOrientationList;
            set => SetProperty(ref _viewOrientationList, value);
        }
        private KeyValuePair<int, string> _selectViewOrientation;
        public KeyValuePair<int, string> SelectViewOrientation
        {
            get => _selectViewOrientation;
            set
            {
                SetProperty(ref _selectViewOrientation, value);
            }
        }
        private double _batchReconSliceThickness;

        public double BatchReconSliceThickness
        {
            get => _batchReconSliceThickness; 
            set => SetProperty(ref _batchReconSliceThickness, value);
        }
        private double _batchReconFov;

        public double BatchReconFov
        {
            get => _batchReconFov;
            set => SetProperty(ref _batchReconFov, value);
        }

        public BatchReconViewModel()
        {
            ViewOrientationList = EnumExtension.EnumToList(typeof(ViewOrientationType));
            SelectViewOrientation = ViewOrientationList[0];
            Commands.Add(CommandName.BatchRecon, new DelegateCommand(BatchReconCommand));
            Commands.Add(CommandName.BatchSave, new DelegateCommand(BatchReconSaveCommand));
            _vm3d = CTS.Global.ServiceProvider.GetService<Image3DViewModel>();       
        }
        private void BatchReconCommand()
        {

        }
        private void BatchReconSaveCommand()
        {

        }
    }
}
