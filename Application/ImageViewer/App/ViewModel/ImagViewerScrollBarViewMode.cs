using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ImageViewer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer.ViewModel
{
    public class ImagViewerScrollBarViewMode:BaseViewModel
    {
        private readonly Image2DViewModel? _image2dViewModel;

        private int _smallChange = 1;
        public int SmallChange
        {
            get => _smallChange;
            set => SetProperty(ref _smallChange, value);
        }

        private int _minimum = 1;
        public int Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value);
        }

        private int _maximum = 10;
        public int Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        private int _currentIndex = 1;
        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }

        private int _lastIndex = -1;
        public int LastIndex
        {
            get => _lastIndex;
            set => SetProperty(ref _lastIndex, value);
        }

        private bool IsScrollLeftButtonDown = false;

        public ImagViewerScrollBarViewMode()
        {
            _image2dViewModel = CTS.Global.ServiceProvider.GetService<Image2DViewModel>();
            Commands.Add(CommandName.COMMAND_PREVIEW_MOUSE_LEFT_BUTTON_DOWN, new DelegateCommand(PreviewMouseLeftButtonDown));
            Commands.Add(CommandName.COMMAND_PREVIEW_MOUSE_LEFT_BUTTON_UP, new DelegateCommand(PreviewMouseLeftButtonUp));
            Commands.Add(CommandName.COMMAND_VALUE_CHANGED, new DelegateCommand(ValueChanged));
            Commands.Add(CommandName.COMMAND_MOUSE_LEAVE, new DelegateCommand(MouseLeave));
            _image2dViewModel.CurrentImageViewer.SliceIndexChanged += CurrentImageViewer_SliceIndexChanged;
            _image2dViewModel.GetSeriesImageCountEvent += UpdateScrollBarMaximum;
        }
        private void MouseLeave() 
        {
            IsScrollLeftButtonDown = false;
        }
        private void UpdateScrollBarMaximum(int seriesimagecount) 
        {
            if (seriesimagecount > 1)
            {
                Minimum = 0;
                Maximum = seriesimagecount;
            }else
            {
                Minimum = 0;
                Maximum = 0;
            }
        }
        private void CurrentImageViewer_SliceIndexChanged(object? sender, (int, double, int) e)
        {
            if (e.Item3 > 1&& !IsScrollLeftButtonDown)
            {
                CurrentIndex = e.Item1;
            }
            if (e.Item3 > 1&& Maximum !=e.Item3)
            {
                UpdateScrollBarMaximum(e.Item3);
            }
        }

        public void PreviewMouseLeftButtonDown()
        {
            IsScrollLeftButtonDown = true;
        }

        public void PreviewMouseLeftButtonUp()
        {
            IsScrollLeftButtonDown = false;
        }

        private void ValueChanged()
        {
            if (IsScrollLeftButtonDown && CurrentIndex != LastIndex)
            {
                _image2dViewModel.CurrentImageViewer.SetSliceIndex(CurrentIndex);
                LastIndex = CurrentIndex;         
            }
        }
    }

  
}
