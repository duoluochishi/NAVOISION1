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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NV.CT.Print.Extensions
{
    public static class BitmapSourceConvertHelper
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert Bitmap to BitmapSource
        /// </summary>
        /// <param name="image">Bitmap</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource ToBitmapSource(System.Drawing.Bitmap image)
        {
            IntPtr ptr = image.GetHbitmap();
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr);
            return bitmapSource;
        }

    }
}
