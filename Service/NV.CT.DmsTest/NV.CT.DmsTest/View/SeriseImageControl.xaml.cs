//using NV.MPS.ImageControl.Commands.Args;
//using NV.MPS.ImageControl;
//using NV.MPS.ImageIO;
//using NV.MPS.ImageRender;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

//using NV.MPS.DICOM.Toolkit;
using System;
using System.Diagnostics;

namespace NV.CT.DmsTest.View
{
    /// <summary>
    /// SeriseImageControl.xaml 的交互逻辑
    /// </summary>
    public partial class SeriseImageControl : Grid
    {
        //ImageControl imageControl = new ImageControl();
        public SeriseImageControl()
        {
            InitializeComponent();
            //this.ImageList.Content = imageControl;
            //this.ImageList.Background = Brushes.Black;
        }


        //int imageCount = 0;

        //public FileStream? ImageFileStream = null;
        //public bool OpenImageFile(string filePath)
        //{
        //    if (File.Exists(filePath))
        //    {
        //        ImageFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        //        return true;
        //    }

        //    return false;
        //}
       
        //public bool CloseImageFile()
        //{
        //    if (ImageFileStream != null)
        //    {
        //        ImageFileStream.Dispose();
        //        ImageFileStream = null;
        //    }
           
        //    return true;
        //}

        /// <summary>
        /// 添加Image图像
        /// </summary>
        /// <param name="fileOffset">图像文件偏移</param>
        /// <returns></returns>
        //public bool AddImage(string filePath, int fileOffset)
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    if (!File.Exists(filePath))
        //    {
        //      return false;
        //    }
        //    ImageFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            
        //    int imageSize = 10240 * 288 * 2;
        //    TwoDImageSource ?imageSource = null;
        //    imageSource = new TwoDImageSource();
        //    byte[] data = new byte[imageSize];

        //    ImageFileStream.Seek(fileOffset * imageSize, SeekOrigin.Begin);
        //    ImageFileStream.Read(data, 0, data.Length);
        //    ImageFileStream.Dispose();
            

        //    ImageDataParser imageDataParser = new ImageDataParser();
        //    ImageData imageData = imageDataParser.ParseImageByRawData(data);
        //    imageSource.LoadImage(imageData);
        //    imageControl.LoadImage(imageSource);
        //    stopwatch.Stop();

        //    Console.WriteLine("LoadImage time is :" + stopwatch.ElapsedMilliseconds + "ms");

        //    return true;
        //}
        
        //public void ClearImage()
        //{
            
        //}

       


        //private void SaveDrived_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveDerivedCommandArgs commandArgs = new SaveDerivedCommandArgs();
        //    imageControl.OnCommand(commandArgs);

        //}

        //private void Zoom_Click(object sender, RoutedEventArgs e)
        //{
        //    ImageZoomCommandArgs commandArgs = new ImageZoomCommandArgs();
        //    imageControl.OnCommand(commandArgs);
        //}

        //private void WL_Click(object sender, RoutedEventArgs e)
        //{
        //    ImageWLCommandArgs commandArgs = new ImageWLCommandArgs();
        //    commandArgs.Step = 100;
        //    imageControl.OnCommand(commandArgs);
        //}

        //private void Invert_Click(object sender, RoutedEventArgs e)
        //{
        //    ImageInvertCommandArgs commandArgs = new ImageInvertCommandArgs();
        //    imageControl.OnCommand(commandArgs);
        //}

        //private void Rotate_Click(object sender, RoutedEventArgs e)
        //{
        //    ImageRotateCommandArgs commandArgs = new ImageRotateCommandArgs();
        //    imageControl.OnCommand(commandArgs);

        //}

        //private void Pan_Click(object sender, RoutedEventArgs e)
        //{
        //    ImagePanCommandArgs commandArgs = new ImagePanCommandArgs();
        //    imageControl.OnCommand(commandArgs);
        //}

        //private void VerticalFlip_Click(object sender, RoutedEventArgs e)
        //{
        //    ImageFlipCommandArgs commandArgs = new ImageFlipCommandArgs(true, false);
        //    //imageControl.OnCommand(commandArgs);
        //}

        //private void HorizontalFlip_Click(object sender, RoutedEventArgs e)
        //{
        //    ImageFlipCommandArgs commandArgs = new ImageFlipCommandArgs(false, true);
        //    //imageControl.OnCommand(commandArgs);
        //}

        //private void Enhance_Click(object sender, RoutedEventArgs e)
        //{
        //    ImageEnhanceCommandArgs commandArgs = new ImageEnhanceCommandArgs();
        //    //imageControl.OnCommand(commandArgs);
        //}
    }
}
