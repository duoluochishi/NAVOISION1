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
// <copyright file="AddEditFolderWindow.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

using NV.CT.PatientManagement.Models;
using System.Windows.Input;

namespace NV.CT.PatientManagement.View.English;

public partial class AddEditFolderWindow
{
    private readonly AddEditFolderViewModel? _viewModel;

    public AddEditFolderWindow()
    {
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        InitializeComponent();

        MouseDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };                

        _viewModel = Global.Instance.ServiceProvider.GetRequiredService<AddEditFolderViewModel>();
        DataContext = _viewModel;

    }

    public void SetDefaultInputFocus()
    {
        txtFolderName.Focus();
    }

}