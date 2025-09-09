//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Examination.Layout;

public partial class ProtocolSelectionControl
{
    //private readonly IDataSync? _dataSync;

    public ProtocolSelectionControl()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<ProtocolSelectMainViewModel>();

        //_dataSync = CTS.Global.ServiceProvider.GetService<IDataSync>();

        //if (_dataSync is not null)
        //{
        //    _dataSync.SelectPatientTypeChanged += DataSync_SelectPatientTypeChanged;
        //}
    }

    //private void DataSync_SelectPatientTypeChanged(object? sender, string e)
    //{   
    //        var newIndex = Int32.Parse(e);
    //        if (TabControl.SelectedIndex == newIndex)
    //            return;

    //        if (newIndex == 0)
    //        {
    //            tabAdult.IsSelected = true;
    //        }
    //        else
    //        {
    //            tabChild.IsSelected = true;
    //        }    
    //}
}