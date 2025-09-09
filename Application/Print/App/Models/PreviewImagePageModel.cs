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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NV.CT.Print.Models
{
    public class PreviewImagePageModel : BaseViewModel
    {
        //仅在列表中显示序号
        private int _number;
        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        private bool _isChoosen;
        public bool IsChoosen
        {
            get => _isChoosen;
            set => SetProperty(ref _isChoosen, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private BitmapSource? _imageData;
        public BitmapSource? ImageData
        {
            get => _imageData;
            set => SetProperty(ref _imageData, value);
        }

        private ICommand? _mouseLeftButtonDownCommand;
        public ICommand? MouseLeftButtonDownCommand
        {
            get => _mouseLeftButtonDownCommand;
            set => SetProperty(ref _mouseLeftButtonDownCommand, value);
        }
    }
}
