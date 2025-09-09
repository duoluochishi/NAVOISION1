using NV.CT.ImageViewer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer.Extensions
{
    public class CommonMethod
    {
        public static void ShowCustomWindow(Type windowType)
        {
            var customWindow = CTS.Global.ServiceProvider.GetService(windowType);

            if (customWindow is Window windowInstance)
            {
                WindowDialogShow.Show(windowInstance);
            }
        }

        public static void ShowCustomWWWLWindow(ViewScene viewScene)
        {
           var _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();

            if (_customWwwlWindow is null)
            {
                _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();
            }

            if (_customWwwlWindow != null)
            {
                _customWwwlWindow.SetScene(viewScene);
                WindowDialogShow.Show(_customWwwlWindow);
            }
        }
        public static void ShowQRWindow(System.Drawing.Bitmap bitMap)
        {
            var _qrWindow = CTS.Global.ServiceProvider?.GetRequiredService<QRDialog>();

            if (_qrWindow != null)
            {
                _qrWindow.SetBitmap(bitMap);
                WindowDialogShow.Show(_qrWindow);
            }
        }
    }
}
