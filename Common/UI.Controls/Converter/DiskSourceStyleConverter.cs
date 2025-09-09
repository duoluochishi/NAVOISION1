using NV.CT.CTS.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter
{
    public class DiskSourceStyleConverter : BasicsConverter
    {
        private const string RESOURCE_NAME_ICON_CD = "IconCD";
        private const string RESOURCE_NAME_ICON_LOCALPATH = "IconLocalPath";
        private const string RESOURCE_NAME_ICON_USB = "IconUSB";

        /// <summary>
        /// 转换DiskSourceStyle为Style
        /// </summary>
        /// <param name="value">必须是三者当中的一个：CDROM、LocalPath、USB</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">N.A.</param>
        /// <param name="culture"></param>
        /// <returns>Style?</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || value is not TargetDiskType)
            {
                return null;
            }

            var targetDiskType = (TargetDiskType)value;
            Style? resultStyle;
            switch(targetDiskType)
            {
                case TargetDiskType.CDROM:
                    resultStyle = (Style)Application.Current.Resources[RESOURCE_NAME_ICON_CD];
                    break;
                case TargetDiskType.LocalPath:
                    resultStyle = (Style)Application.Current.Resources[RESOURCE_NAME_ICON_LOCALPATH];
                    break;
                case TargetDiskType.USB:
                    resultStyle = (Style)Application.Current.Resources[RESOURCE_NAME_ICON_USB];
                    break;
                default:
                    resultStyle = null;
                    break;
            }
            return resultStyle;
        }
    }
}
