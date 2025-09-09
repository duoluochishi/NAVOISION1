using Microsoft.VisualBasic.Logging;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ThoughtWorks.QRCode.Codec;
using Size = System.Drawing.Size;
using System.Runtime.Serialization;

namespace NV.CT.Screenshot;

public class _Screenshot
{
    private const string IISPath = @"WebRoot\";
    /// <summary>
    /// 截图后的图像通过此委托返回
    /// </summary>
    public static Action<BitmapSource>? ReturnScreenShotEvent;

    public static string LocalIPAddress  = "192.168.137.1";

    public _Screenshot()
    {
            
    }
    /// <summary>
    /// 获取屏幕当前截图.
    /// </summary>
    public static BitmapSource? CaptureAllScreens()
    {
        return CaptureRegion(new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight));

        ////测试用主屏幕,截取屏幕区域
        //var rect = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop
        //    , SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

        //rect = new Rect(0, 0, 1500, 1000);
        //return CaptureRegion(rect);
    }

    /// <summary>
    /// 根据传入的矩形截取屏幕上的一部分
    /// </summary>
    public static BitmapSource? CaptureRegion(Rect rect)
    {
        if (rect.Width < 20 || rect.Height < 20)
        {
            return null;
        }

        using var bitmap = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb);
        var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen((int)rect.X, (int)rect.Y, 0, 0, new Size((int)rect.Size.Width, (int)rect.Size.Height), CopyPixelOperation.SourceCopy);
        return bitmap.ToBitmapSource();
    }

    /// <summary>
    /// 开始截图.
    /// </summary>
    /// <param name="options">对透明度以及框的颜色进行设置.</param>
    /// <returns>The <see cref="BitmapSource"/>.</returns>
    public static BitmapSource? CaptureRegion(ScreenshotOptions? options = null)
    {
        BitmapSource? bitmapSource = null;
        options = options ?? new ScreenshotOptions();
        var bitmap = CaptureAllScreens();
        var left = SystemParameters.VirtualScreenLeft;
        var top = SystemParameters.VirtualScreenTop;

        var right = left + SystemParameters.VirtualScreenWidth;
        var bottom = right + SystemParameters.VirtualScreenHeight;

        ////TODO:debug only 截图区域设定
        //var right = left + SystemParameters.PrimaryScreenWidth;
        //var bottom = right + SystemParameters.PrimaryScreenHeight;

        var window = new RegionSelectionWindow
        {
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            BorderThickness = new Thickness(0),
            BackgroundImage =
            {
                Source = bitmap,
                Opacity = options.BackgroundOpacity
            },
            InnerBorder = { BorderBrush = options.SelectionRectangleBorderBrush },
            Left = left,
            Top = top,
            Width = right - left,
            Height = bottom - top,
            Topmost = true
        };
        window.ShowDialog();
        if (window.SelectedRegion == null)
        {
            if (bitmapSource != null)
                ReturnScreenShotEvent?.Invoke(bitmapSource);
        }
        else
        {
            if (window.SelectedRegion == new Rect(0.1, 0.1, 0.1, 0.1))
            {
                bitmapSource = bitmap;
                if (bitmapSource != null)
                {
                    System.Windows.Clipboard.SetImage(bitmapSource);
                    //ReturnScreenShotEvent?.BeginInvoke(bitmapSource, null, null);
                    ReturnScreenShotEvent?.Invoke(bitmapSource);
                }
            }
            else
            {
                // bitmapSource = GetBitmapRegion(bitmap, window.SelectedRegion.Value);
                bitmapSource = CaptureRegion(window.SelectedRegion.Value);

                if (bitmapSource != null)
                {
                    System.Windows.Clipboard.SetImage(bitmapSource);
                    //ReturnScreenShotEvent?.BeginInvoke(bitmapSource, null, null);
                    ReturnScreenShotEvent?.Invoke(bitmapSource);
                }
            }
        }

        return bitmapSource;
    }

    /// <summary>
    /// 注册热键.
    /// </summary>
    /// <param name="window">接收消息的窗口.</param>
    /// <param name="modifiers">功能键.</param>
    /// <param name="key">附加按键.</param>
    public static void RegisterHotKey(Window window, ModifierKeys modifiers, Keys key)
    {
        HotKey.Register(window, modifiers, key, delegate
        {
            CaptureRegion();
        });
    }

    /// <summary>
    /// 从图片中截取一部分
    /// </summary>
    private static BitmapSource? GetBitmapRegion(BitmapSource bitmap, Rect rect)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return null;
        }

        return new CroppedBitmap(bitmap, new Int32Rect
        {
            X = (int)rect.X,
            Y = (int)rect.Y,
            Width = (int)rect.Width,
            Height = (int)rect.Height
        });
    }
    public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
    {
        Bitmap bitmap;
        using (var outStream = new MemoryStream())
        {
            BitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapsource));
            enc.Save(outStream);
            bitmap = new Bitmap(outStream);
        }
        return bitmap;
    }
    public static Bitmap ConvertViaStream(BitmapImage bitmapImage)
    {
        using (var stream = new MemoryStream())
        {
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(stream);
            return new System.Drawing.Bitmap(stream);
        }
    }

    public static Window GetParentWindow(ContentControl parent)
    {
        if (parent is Window)
            return (Window)parent;
        else
        {
            try
            {
                return Window.GetWindow(parent);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public static void SaveBitmapSource(BitmapSource bitmapsource, string path)
    {
        var fullName = System.IO.Directory.GetParent(path)?.FullName;
        if (string.IsNullOrEmpty(fullName))   
            return;
        if (!Directory.Exists(fullName))
        {
            Directory.CreateDirectory(fullName);
        }
        BitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            encoder.Save(stream);
        }
    }
    private static string RunApp(string filename, string arguments, bool recordLog)
    {
        try
        {
            if (recordLog)
            {
                Trace.WriteLine(filename + " " + arguments);
            }
            Process proc = new Process();
            proc.StartInfo.FileName = filename;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(proc.StandardOutput.BaseStream, Encoding.Default))
            {
                //string txt = sr.ReadToEnd();
                //sr.Close();
                //if (recordLog)
                //{
                //    Trace.WriteLine(txt);
                //}
                //if (!proc.HasExited)
                //{
                //    proc.Kill();
                //}
                //上面标记的是原文，下面是我自己调试错误后自行修改的
                Thread.Sleep(100);           //貌似调用系统的nslookup还未返回数据或者数据未编码完成，程序就已经跳过直接执行
                                             //txt = sr.ReadToEnd()了，导致返回的数据为空，故睡眠令硬件反应
                if (!proc.HasExited)         //在无参数调用nslookup后，可以继续输入命令继续操作，如果进程未停止就直接执行
                {                            //txt = sr.ReadToEnd()程序就在等待输入，而且又无法输入，直接掐住无法继续运行
                    proc.Kill();
                }
                string txt = sr.ReadToEnd();
                sr.Close();
                if (recordLog)
                    Trace.WriteLine(txt);
                return txt;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            return ex.Message;
        }
    }
    private static string GetLocalIP()
    {
        string result = RunApp("route", "print", true);
        if (result.Contains("192.168.137.1")) return "192.168.137.1";
        Match m = Regex.Match(result, @"0.0.0.0\s+0.0.0.0\s+(\d+.\d+.\d+.\d+)\s+(\d+.\d+.\d+.\d+)");
        if (m.Success)
        {
            return m.Groups[2].Value.Trim();
        }
        else
        {
            try
            {
                System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient();
                c.Connect("www.baidu.com", 80);
                var ipEndPoint= c.Client.LocalEndPoint;
                if (ipEndPoint != null)
                {
                    string ip = ((System.Net.IPEndPoint)ipEndPoint).Address.ToString();
                    c.Close();
                    return ip.Trim();
                }else
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
    public static string GetMachineIP() 
    {
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            var ipEndPoint= socket.LocalEndPoint;
            if (ipEndPoint != null) 
            {
                return ((IPEndPoint)ipEndPoint).Address.ToString();
            } 
            return string.Empty;
        }
    }
    public static Bitmap? StartScreenshot(string iisPath,BitmapSource? bitmapSource)
    {
        try
        {
            //BitmapSource bitmapSource2=     Screenshot.Screenshot.CaptureRegion();
            //SaveBitmapSource(bitmapSource2, iisPath + "\\" + DateTime.Now.ToString("HHmmss.ffff") + ".png");
            if (!Directory.Exists(iisPath))
            {
                Directory.CreateDirectory(iisPath);
            }
            string dateTime = DateTime.Now.ToString("yyyyMMdd-HH-mm-ss-fff", System.Globalization.CultureInfo.InvariantCulture);
            if (bitmapSource !=null)
            {
                string strFileName = dateTime + ".png";
                SaveBitmapSource(bitmapSource, iisPath + strFileName);
                QRCodeEncoder encoder = new QRCodeEncoder();
                encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;//编码方式(注意：BYTE能支持中文，ALPHA_NUMERIC扫描出来的都是数字)
                encoder.QRCodeScale = 24;//大小(值越大生成的二维码图片像素越高)
                encoder.QRCodeVersion = 0;//版本(注意：设置为0主要是防止编码的字符串太长时发生错误)
                encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//错误效验、错误更正(有4个等级)
                encoder.QRCodeBackgroundColor = System.Drawing.Color.White;
                encoder.QRCodeForegroundColor = System.Drawing.Color.Black;
                //string qrdata = "http://192.168.137.1:8088/" + strFileName;
                string ip = GetMachineIP();
                if (string.IsNullOrEmpty(ip))
                {
                    ip = LocalIPAddress;
                }else
                {
                    LocalIPAddress = ip;
                }
                string qrdata = "http://" + ip + ":30070/" + strFileName;
                Bitmap bmpCode = encoder.Encode(qrdata.ToString());
                BitmapImage LogoImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.Screenshot;component/Resources/logo.png", UriKind.Absolute));
                Bitmap bmpLogo = ConvertViaStream(LogoImage);
                Bitmap bmpTmp = new Bitmap(bmpCode.Width - 1, bmpCode.Height - 1);
                using (Graphics gr = Graphics.FromImage(bmpTmp))
                {
                    gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    gr.DrawImage(bmpCode, 0, 0);
                    gr.DrawImage(bmpLogo, bmpCode.Width * 2 / 5, bmpCode.Height * 2 / 5, bmpCode.Width / 5, bmpCode.Height / 5);
                }
                return bmpTmp;
            }else
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}