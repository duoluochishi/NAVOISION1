using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer.ViewModel
{
    public class CustomRotateViewModel:BaseViewModel
    {
        private readonly Image2DViewModel? _image2dViewModel;
        private int _angle;

        public int Angle
        {
            get => _angle;
            set => SetProperty(ref _angle, value);
        }
        private bool _isButtonEnabled;

        public bool IsButtonEnabled
        {
            get => _isButtonEnabled; 
            set => SetProperty(ref _isButtonEnabled, value); 
        }

        public CustomRotateViewModel()
        {
            _image2dViewModel = CTS.Global.ServiceProvider.GetService<Image2DViewModel>();

            Commands.Add("SetAngle", new DelegateCommand<int?>(degree =>
            {
              _image2dViewModel?.CurrentImageViewer.Rotate(degree.GetValueOrDefault());      
            }));
                       
           
        }
       
    }
}
