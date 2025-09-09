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

namespace NV.CT.PatientManagement.View.English;

public partial class InstanceViewControl
{
    public InstanceViewControl()
    {
        InitializeComponent();
        DataContext = Global.Instance.ServiceProvider.GetRequiredService<InstanceViewModel>();

        this.rbInvert.IsChecked = true;
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
        //RadioButton radioButton = sender as RadioButton;
        if (sender is RadioButton radioButton)
        {
            foreach (UIElement item in grdMain.Children)
            {
                if (item is Grid)
                {
                    Grid grid = (Grid)item;
                    if (grid.Name == radioButton?.Tag.ToString())
                    {
                        grid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid.Visibility = Visibility.Collapsed;
                    }
                }
                else if (item is ContentControl)
                {
                    ContentControl contentControl = (ContentControl)item;
                    if (contentControl.Name == radioButton?.Tag.ToString())
                    {
                        contentControl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        contentControl.Visibility = Visibility.Collapsed;
                    }
                }
            }

        }
    }
}