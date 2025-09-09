//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="VStudy.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

using System.Windows.Input;

namespace NV.CT.PatientManagement.View.English;

public partial class FilmPlayControl
{
    public FilmPlayViewModel FilmPlayViewModel { get; set; }
    public FilmPlayControl()
    {
        InitializeComponent();
        MouseDown += delegate
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        };
        Closed += FilmPlayControl_Closed;
        FilmPlayViewModel = Global.Instance.ServiceProvider.GetRequiredService<FilmPlayViewModel>();
        DataContext = FilmPlayViewModel;
    }

    private void FilmPlayControl_Closed(object? sender, EventArgs e)
    {
        ////电影播放暂时不需要了！

        //if (Tag is InstanceViewControl instanceViewList)
        //{
        //    var image = instanceViewList.rbFilm.Content as Image;
        //    instanceViewList.rbFilm.IsChecked = false;
        //    if (image != null)
        //    {
        //        string str = @"pack://application:,,,/NV.CT.UI.Controls;component/Icons/i_film_0.png";
        //        ImageSourceConverter imageSourceConverter = new ImageSourceConverter();

        //        var imageSource = imageSourceConverter.ConvertFrom(str);
        //        if (imageSource is not null)
        //        {
        //            image.Source = (ImageSource)imageSource;
        //        }
        //    }
        //}
    }

    private void btn_close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}