using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter
{
    public class FolderStyleConverter : BasicsConverter
    {
        private const string RESOURCE_NAME_ICON_DISK = "IconLocalPath";
        private const string RESOURCE_NAME_ICON_FOLDER = "IconFolder";

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
            if (value is bool isLogicalDisk)
            {
                return isLogicalDisk ? (Style)Application.Current.Resources[RESOURCE_NAME_ICON_DISK] : (Style)Application.Current.Resources[RESOURCE_NAME_ICON_FOLDER];
            }
            else
            {
                return null;
            }
        }
    }
}
